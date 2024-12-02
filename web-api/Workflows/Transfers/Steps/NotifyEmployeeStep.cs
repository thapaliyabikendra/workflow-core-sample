using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class NotifyEmployeeStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from previous steps
    public string EmployeeEmail { get; set; }
    public string TransferOutcome { get; set; }
    public string ApprovalStatus { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Simulate sending a notification (e.g., email, SMS, etc.)
        await SendNotificationAsync(EmployeeEmail, TransferOutcome);

        // Log successful notification
        //Console.WriteLine($"{TaskId} - Notification sent to employee: {EmployeeEmail} about Approval Status {ApprovalStatus}.");

        return ExecutionResult.Next(); // End the workflow or continue with more steps
    }

    private Task SendNotificationAsync(string email, string outcome)
    {
        return Task.CompletedTask;
    }
}
