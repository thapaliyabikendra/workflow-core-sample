using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;
public class TriggerUiPathJobStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId
    public string UiPathJobId { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Log the unique TaskId and job trigger
        Console.WriteLine($"[{TaskId}] Triggering UiPath job.");

        // Simulate triggering UiPath job
        UiPathJobId = await TriggerUiPathJobAsync();
        Console.WriteLine($"JobId - [{UiPathJobId}] Triggering UiPath job.");

        // Pass the UiPath Job ID to the next step
        return ExecutionResult.Next(); // Proceed to the next step
    }

    private Task<string> TriggerUiPathJobAsync()
    {
        // Simulate calling UiPath Orchestrator to trigger the job
        return Task.FromResult("UiPathJob1234"); // Return a mock Job ID
    }
}
