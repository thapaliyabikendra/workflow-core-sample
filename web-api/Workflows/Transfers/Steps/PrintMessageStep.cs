using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class PrintMessageStep : StepBody
{
    public string Message { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        Console.WriteLine(Message);
        return ExecutionResult.Next();
    }
}