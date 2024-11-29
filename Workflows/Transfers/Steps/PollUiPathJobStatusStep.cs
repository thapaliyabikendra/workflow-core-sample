using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class PollUiPathJobStatusStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from previous step
    public string UiPathJobId { get; set; } // Job ID passed from previous step
    public string JobStatus { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        Console.WriteLine($"[{TaskId}] Polling UiPath job status.");

        JobStatus = await GetUiPathJobStatusAsync(UiPathJobId);

        if (JobStatus == "Completed")
        {
            Console.WriteLine($"[{TaskId}] UiPath job completed successfully.");
        }

        return ExecutionResult.Next(); // Proceed to next step
    }

    private Task<string> GetUiPathJobStatusAsync(string jobId)
    {
        // Simulate checking UiPath job status
        return Task.FromResult("Completed"); // Simulating a "Completed" job status
    }
}
