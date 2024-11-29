using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class CallBPMApiStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from the previous step

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Simulate calling the BPM API for Maker/Checker approval
        Console.WriteLine($"[{TaskId}] Calling BPM API for approval/rejection...");

        await CallBPMApiAsync();

        // Proceed to the next step if approved
        return ExecutionResult.Next();
    }

    private Task CallBPMApiAsync()
    {
        // Simulate the BPM API response (in a real case, this would be a web API call)
        return Task.CompletedTask;  // Simulating an approved status
    }
}