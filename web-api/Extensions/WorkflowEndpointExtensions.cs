using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.Search;

namespace ACMS.WebApi.Extensions;

public static class WorkflowEndpointExtensions
{
    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map the GET /api/workflows endpoint
        endpoints.MapGet("/api/workflows", async (
            ISearchIndex searchService,
            ILogger<IEndpointRouteBuilder> logger,
            string terms, WorkflowStatus? status, string type, DateTime? createdFrom, DateTime? createdTo, int skip, int take = 10
        ) =>
        {
            logger.LogInformation("Received request for GET /api/workflows with terms: {Terms}, status: {Status}, type: {Type}", terms, status, type);

            try
            {
                var filters = searchService.BuildSearchFilters(status, type, createdFrom, createdTo);
                var result = await searchService.Search(terms, skip, take, filters.ToArray());

                logger.LogInformation("Search completed, found {Count} results.", result.Total);
                return Results.Json(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing GET /api/workflows");
                return Results.InternalServerError("An error occurred while fetching workflows.");
            }
        });

        // Map the GET /api/workflows/{id} endpoint
        endpoints.MapGet("/api/workflows/{id}", async (string id, IPersistenceProvider workflowStore, ILogger<IEndpointRouteBuilder> logger) =>
        {
            logger.LogInformation("Received request for GET /api/workflows/{Id}", id);

            try
            {
                var result = await workflowStore.GetWorkflowInstance(id);

                if (result != null)
                {
                    logger.LogInformation("Workflow instance with ID {Id} found.", id);
                    return Results.Json(result);
                }
                else
                {
                    logger.LogWarning("Workflow instance with ID {Id} not found.", id);
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching workflow instance {Id}.", id);
                return Results.InternalServerError("An error occurred while fetching the workflow instance.");
            }
        });

        // Map the POST /api/workflows/{id}/{version} endpoint
        endpoints.MapPost("/api/workflows/{id}/{version?}", async (
            string id, int? version, [FromBody] object data,
            IWorkflowRegistry registry, IWorkflowController workflowService, ILogger<IEndpointRouteBuilder> logger) =>
        {
            logger.LogInformation("Received request for POST /api/workflows/{Id}/{Version}", id, version);

            try
            {
                var def = registry.GetDefinition(id, version);
                if (def == null)
                {
                    logger.LogWarning("Workflow definition for ID {Id} and version {Version} not found.", id, version);
                    return Results.BadRequest($"Workflow definition {id} for version {version} not found");
                }

                string workflowId = null;
                if (data != null && def.DataType != null)
                {
                    var dataStr = JsonConvert.SerializeObject(data);
                    var dataObj = JsonConvert.DeserializeObject(dataStr, def.DataType);
                    workflowId = await workflowService.StartWorkflow(id, version, dataObj);
                    logger.LogInformation("Started workflow {WorkflowId} for ID {Id}, version {Version}.", workflowId, id, version);
                }
                else
                {
                    workflowId = await workflowService.StartWorkflow(id, version, null);
                    logger.LogInformation("Started workflow {WorkflowId} for ID {Id}, version {Version} without data.", workflowId, id, version);
                }

                return Results.Ok(workflowId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while starting workflow {Id} version {Version}.", id, version);
                return Results.InternalServerError("An error occurred while starting the workflow.");
            }
        });

        // Map the PUT /api/workflows/{id}/suspend endpoint
        endpoints.MapPut("/api/workflows/{id}/suspend", async (string id, IWorkflowController workflowService, ILogger<IEndpointRouteBuilder> logger) =>
        {
            logger.LogInformation("Received request for PUT /api/workflows/{Id}/suspend", id);

            try
            {
                var success = await workflowService.SuspendWorkflow(id);

                if (success)
                {
                    logger.LogInformation("Successfully suspended workflow with ID {Id}.", id);
                    return Results.Ok();
                }
                else
                {
                    logger.LogWarning("Failed to suspend workflow with ID {Id}.", id);
                    return Results.BadRequest("Failed to suspend workflow");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while suspending workflow {Id}.", id);
                return Results.InternalServerError("An error occurred while suspending the workflow.");
            }
        });

        // Map the PUT /api/workflows/{id}/resume endpoint
        endpoints.MapPut("/api/workflows/{id}/resume", async (string id, IWorkflowController workflowService, ILogger<IEndpointRouteBuilder> logger) =>
        {
            logger.LogInformation("Received request for PUT /api/workflows/{Id}/resume", id);

            try
            {
                var success = await workflowService.ResumeWorkflow(id);

                if (success)
                {
                    logger.LogInformation("Successfully resumed workflow with ID {Id}.", id);
                    return Results.Ok();
                }
                else
                {
                    logger.LogWarning("Failed to resume workflow with ID {Id}.", id);
                    return Results.BadRequest("Failed to resume workflow");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while resuming workflow {Id}.", id);
                return Results.InternalServerError("An error occurred while resuming the workflow.");
            }
        });

        // Map the DELETE /api/workflows/{id} endpoint
        endpoints.MapDelete("/api/workflows/{id}", async (string id, IWorkflowController workflowService, ILogger<IEndpointRouteBuilder> logger) =>
        {
            logger.LogInformation("Received request for DELETE /api/workflows/{Id}", id);

            try
            {
                var success = await workflowService.TerminateWorkflow(id);

                if (success)
                {
                    logger.LogInformation("Successfully terminated workflow with ID {Id}.", id);
                    return Results.Ok();
                }
                else
                {
                    logger.LogWarning("Failed to terminate workflow with ID {Id}.", id);
                    return Results.BadRequest("Failed to terminate workflow");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while terminating workflow {Id}.", id);
                return Results.InternalServerError("An error occurred while terminating the workflow.");
            }
        });

        return endpoints;
    }

    public static List<SearchFilter> BuildSearchFilters(
       this ISearchIndex searchService,
       WorkflowStatus? status,
       string type,
       DateTime? createdFrom,
       DateTime? createdTo)
    {
        var filters = new List<SearchFilter>();

        if (status.HasValue)
            filters.Add(StatusFilter.Equals(status.Value));

        if (createdFrom.HasValue)
            filters.Add(DateRangeFilter.After(x => x.CreateTime, createdFrom.Value));

        if (createdTo.HasValue)
            filters.Add(DateRangeFilter.Before(x => x.CreateTime, createdTo.Value));

        if (!string.IsNullOrEmpty(type))
            filters.Add(ScalarFilter.Equals(x => x.WorkflowDefinitionId, type));

        return filters;
    }
}
