using ACMS.WebApi.Models;
using WorkflowCore.Interface;

namespace ACMS.WebApi.Extensions;

public static class WebhookEndpointExtensions
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/webhook", async (HttpContext context, IWorkflowHost workflowHost) =>
        {
            var payload = await context.Request.ReadFromJsonAsync<WebhookPayload>();

            if (string.IsNullOrEmpty(payload?.TaskId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Task Id is required.");
                return;
            }

            // Trigger the event for the workflow
            await workflowHost.PublishEvent("BPMAPIApprovalResponseEvent", payload.TaskId, payload.ApprovalStatus);

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync($"Event for TaskId: {payload.TaskId} triggered with status: {payload.ApprovalStatus}");
        });

        return endpoints;
    }
}