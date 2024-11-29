using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class PollUiPathJobStatusStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from previous step
    public string UiPathJobId { get; set; } 
    public string JobStatus { get; set; }
    public bool IsDataPolled { get; set; }
    public int PollingCount { get; set; }
    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        Console.WriteLine($"[{TaskId}] Polling UiPath job status - Request {PollingCount} - IsDataPolled: {IsDataPolled}.");
        PollingCount += 1;
        JobStatus = await GetUiPathJobStatusAsync(UiPathJobId);
        if (PollingCount == 2) {
            Console.WriteLine($"[{TaskId}] UiPath job completed successfully.");
            IsDataPolled = true;
        }

        return ExecutionResult.Next();
    }

    private Task<string> GetUiPathJobStatusAsync(string jobId)
    {
        // Simulate checking UiPath job status
        return Task.FromResult("Completed"); // Simulating a "Completed" job status
    }
}
