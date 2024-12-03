using WorkflowCore.Interface;

namespace ACMS.WebApi.Middlewares;

public class CustomWorkflowErrorHandler(ILogger<CustomWorkflowErrorHandler> logger) : IWorkflowMiddlewareErrorHandler
{
    public async Task HandleAsync(Exception ex)
    {
        logger.LogError(ex.Message);
        await Task.CompletedTask;
    }
}