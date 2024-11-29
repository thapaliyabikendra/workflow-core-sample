using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class NotifyEmployeeStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from previous steps
    public string EmployeeEmail { get; set; }
    public string TransferOutcome { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Log the notification attempt with TaskId
        Console.WriteLine($"[{TaskId}] Sending notification to employee: {EmployeeEmail}");

        // Simulate sending a notification (e.g., email, SMS, etc.)
        await SendNotificationAsync(EmployeeEmail, TransferOutcome);

        // Log successful notification
        Console.WriteLine($"[{TaskId}] Notification sent to employee.");

        return ExecutionResult.Next(); // End the workflow or continue with more steps
    }

    private Task SendNotificationAsync(string email, string outcome)
    {
        // Simulate sending an email (in a real app, use an email service)
        Console.WriteLine($"Notifying {email} about transfer: {outcome}");
        return Task.CompletedTask;
    }
}
