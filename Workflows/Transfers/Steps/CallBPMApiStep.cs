using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class CallBPMApiStep : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from the previous step
    public string ApprovalStatus { get; set; }  // "Approved" or "Rejected"

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Simulate calling the BPM API for Maker/Checker approval
        Console.WriteLine($"[{TaskId}] Calling BPM API for approval/rejection...");

        ApprovalStatus = await CallBPMApiAsync();

        // Log the approval status with the TaskId
        Console.WriteLine($"[{TaskId}] BPM approval status: {ApprovalStatus}");

        if (ApprovalStatus == "Rejected")
        {
            throw new Exception("Transfer Rejected by BPM");
        }

        // Proceed to the next step if approved
        return ExecutionResult.Next();
    }

    private Task<string> CallBPMApiAsync()
    {
        // Simulate the BPM API response (in a real case, this would be a web API call)
        return Task.FromResult("Approved");  // Simulating an approved status
    }
}