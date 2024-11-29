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
        builder
            .StartWith<GenerateTaskIdStep>()               // Step 1: Generate Task ID
            .Then<CallBPMApiStep>()                       // Step 2: Call BPM API (Maker/Checker)
                .Input(step => step.TaskId, data => data.TaskId)
            .WaitFor("BPMAPIApprovalResponseEvent", (data, context) => "0", data => DateTime.Now)   // Step 3: Wait for BPM API Approval Response event
                .Output(data => data.ApprovalStatus, step => step.EventData)
            .Then(context => Console.WriteLine("BPMAPIApprovalResponseEvent received."))
            .Then<TriggerUiPathJobStep>()                // Step 4: Trigger UiPath job
                .Input(step => step.TaskId, data => data.TaskId)
            .Schedule(data => TimeSpan.FromSeconds(10))
                .Do(schedule => schedule
                    .Recur(data => TimeSpan.FromSeconds(5), data => data.PollingCount > 5)
                        .Do(recur => recur
                            .StartWith<PollUiPathJobStatusStep>()             // Step 5: Poll UiPath job status
                            .Input(step => step.TaskId, data => data.TaskId)
                        )
                )
            .Then<NotifyEmployeeStep>()                  // Step 6: Notify employee
                .Input(step => step.TaskId, data => data.TaskId);
    }
}