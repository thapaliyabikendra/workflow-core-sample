using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Middlewares;

public class LogCorrelationStepMiddleware : IWorkflowStepMiddleware
{
    private readonly ILogger<LogCorrelationStepMiddleware> _log;

    public LogCorrelationStepMiddleware(
        ILogger<LogCorrelationStepMiddleware> log)
    {
        _log = log;
    }

    public async Task<ExecutionResult> HandleAsync(
        IStepExecutionContext context,
        IStepBody body,
        WorkflowStepDelegate next)
    {
        var workflowId = context.Workflow.Id;
        var stepId = context.Step.Id;

        // Uses log scope to add a few attributes to the scope
        using (_log.BeginScope("{@WorkflowId}", workflowId))
        using (_log.BeginScope("{@StepId}", stepId))
        {
            // Calling next ensures step gets executed
            return await next();
        }
    }
}