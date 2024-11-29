using ACMS.WebApi.Models;
using ACMS.WebApi.Workflows.Transfers.Steps;
using WorkflowCore.Interface;

namespace ACMS.WebApi.Workflows.Transfers;
public class EmployeeTransferWorkflow : IWorkflow<EmployeeTransferDataDto>
{
    public string Id => "EmployeeTransferWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<EmployeeTransferDataDto> builder)
    {
        var approvedBranch = builder.CreateBranch()
            .StartWith<TriggerUiPathJobStep>()
                .Input(step => step.TaskId, data => data.TaskId)
                .Output(step => step.UiPathJobId, data => data.UiPathJobId)
            .Schedule(data => TimeSpan.FromSeconds(4))
                .Do(schedule => schedule
                    .Recur(data => TimeSpan.FromSeconds(1), data => data.IsDataPolled || data.PollingCount > 3)
                        .Do(recur => recur
                            .StartWith<PollUiPathJobStatusStep>()
                                .Input(step => step.TaskId, data => data.TaskId)
                                .Input(step => step.UiPathJobId, data => data.UiPathJobId)
                                .Input(step => step.PollingCount, data => data.PollingCount)
                                .Input(step => step.IsDataPolled, data => data.IsDataPolled)
                                .Output(data => data.PollingCount, step => step.PollingCount)
                                .Output(data => data.IsDataPolled, step => step.IsDataPolled)
                            .If(data => data.IsDataPolled)
                                .Do(then => 
                                    then.StartWith<NotifyEmployeeStep>()
                                            .Input(step => step.TaskId, data => data.TaskId)
                                            .Input(step => step.ApprovalStatus, data => data.ApprovalStatus)
                                        .EndWorkflow()
                                        .Then((context) => Console.WriteLine("[] Workflow Ended 1."))
                                )
                        )
                );

        var rejectedBranch = builder.CreateBranch()
            .StartWith<NotifyEmployeeStep>()
                    .Input(step => step.TaskId, data => data.TaskId)
                    .Input(step => step.ApprovalStatus, data => data.ApprovalStatus)
            .EndWorkflow()
                .Then((context) => Console.WriteLine("[] Workflow Ended 2."));

        builder
            .StartWith<GenerateTaskIdStep>()
                .Output(data => data.TaskId, step => step.TaskId)
            .Then<CallBPMApiStep>()
                .Input(step => step.TaskId, data => data.TaskId)
            .WaitFor("BPMAPIApprovalResponseEvent", (data, context) => data.TaskId, data => DateTime.Now)
                .Output(data => data.ApprovalStatus, step => step.EventData)
            .Then((context) => Console.WriteLine("[] BPM API Approval Response Event received."))
            .Decide(data => data.ApprovalStatus)
            .Branch((data, outcome) => data.ApprovalStatus == "Approved", approvedBranch)
            .Branch((data, outcome) => data.ApprovalStatus == "Rejected", rejectedBranch);
    }
}