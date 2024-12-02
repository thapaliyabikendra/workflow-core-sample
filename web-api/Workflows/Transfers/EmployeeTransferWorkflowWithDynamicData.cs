using ACMS.WebApi.Models;
using ACMS.WebApi.Workflows.Transfers.Steps;
using Nest;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;

namespace ACMS.WebApi.Workflows.Transfers;
public class EmployeeTransferWorkflowWithDynamicData : IWorkflow<DynamicData>
{
    public string Id => "EmployeeTransferWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<DynamicData> builder)
    {
        var approvedBranch = builder.CreateBranch()
            .StartWith<TriggerUiPathJobStep>()
                .Input(step => step.TaskId, data => data["TaskId"])
                .Output(data => data["UiPathJobId"], step => step.UiPathJobId)
            //.OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(10))
            .Schedule(data => TimeSpan.FromSeconds(2))
                .Do(schedule => schedule
                    .Recur(data => TimeSpan.FromSeconds(3), data => object.Equals(data["IsDataPolled"], true) || Convert.ToInt32(data["PollingCount"]) > 5)
                        .Do(recur => recur
                            .StartWith<PollUiPathJobStatusStep>()
                                .Input(step => step.IsDataPolled, data => data["IsDataPolled"])
                                .Input(step => step.TaskId, data => data["TaskId"])
                                .Input(step => step.UiPathJobId, data => data["UiPathJobId"])
                                .Input(step => step.PollingCount, data => data["PollingCount"])
                                .Output(data => data["IsDataPolled"], step => step.IsDataPolled)
                                .Output(data => data["PollingCount"], step => step.PollingCount)
                            .If(data => true)
                                .Do(then =>
                                    then.StartWith<NotifyEmployeeStep>()
                                            .Input(step => step.TaskId, data => data["TaskId"])
                                            .Input(step => step.ApprovalStatus, data => data["ApprovalStatus"])
                                        .EndWorkflow()
                                        .Then((context) => Console.WriteLine("[] Workflow Ended 1."))
                                )
                        )
                );

        var rejectedBranch = builder.CreateBranch()
            .StartWith<NotifyEmployeeStep>()
                    .Input(step => step.TaskId, data => data["TaskId"])
                    .Input(step => step.ApprovalStatus, data => data["ApprovalStatus"])
            .EndWorkflow()
                .Then((context) => Console.WriteLine("[] Workflow Ended 2."));

        builder
            //.UseDefaultErrorBehavior(WorkflowErrorHandling.Terminate)
            .StartWith<GenerateTaskIdStep>()
                .Output(data => data["TaskId"], step => step.TaskId)
            .Then<CallBPMApiStep>()
                .Input(step => step.TaskId, data => data["TaskId"])
                .Input(step => step.UserId, data => data["UserId"])
                .Output(data => data["ApiResponse1"], step => step.Response)
            //.OnError(WorkflowErrorHandling.Terminate)
            .If(data => Convert.ToBoolean(data.ToJObject("ApiResponse1")["success"])) // if relevant http status code is not returned
            .Do(s =>
                 s.Then(context => Console.WriteLine("[] BPM API Approval Request failed."))
                 .EndWorkflow()
                 .Then((context) => Console.WriteLine("[] Workflow Ended 3."))
            )
            //.CancelCondition(data => true) // will be active for whole workflow
            .Then(context => Console.WriteLine("[] Waiting for BPM API Approval Response Event."))
            .WaitFor("BPMAPIApprovalResponseEvent", (data, context) => data["TaskId"] as string, data => DateTime.Now)
                .Output(data => data["ApprovalStatus"], step => step.EventData)
            .Then(context => Console.WriteLine("[] BPM API Approval Response Event received."))
            .Decide(data => data["ApprovalStatus"])
            .Branch((data, outcome) => object.Equals(data["ApprovalStatus"], "Approved"), approvedBranch)
            .Branch((data, outcome) => object.Equals(data["ApprovalStatus"], "Rejected"), rejectedBranch);
    }
}