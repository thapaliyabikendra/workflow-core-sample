using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class GenerateTaskIdStep : StepBody
{
    public string TaskId { get; set; }  // Renamed UniqueId to TaskId

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        // Generate a unique Task ID for the workflow
        TaskId = Guid.NewGuid().ToString();

        // Log the generated Task ID (useful for debugging)
        Console.WriteLine($"{TaskId} - Generated Task ID");

        // Pass the TaskId to the next step
        return ExecutionResult.Next(); // Continue to the next step
    }
}