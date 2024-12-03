using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class PrintMessageStep(ILogger<PrintMessageStep> logger) : StepBody
{
    public string Message { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        logger.LogInformation(Message);
        return ExecutionResult.Next();
    }
}