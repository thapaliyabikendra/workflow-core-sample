using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            string terms, WorkflowStatus? status, string type, DateTime? createdFrom, DateTime? createdTo, int skip, int take = 10) =>
        {
            var filters = searchService.BuildSearchFilters(status, type, createdFrom, createdTo);
            var result = await searchService.Search(terms, skip, take, filters.ToArray());
            return Results.Json(result);
        });

        // Map the GET /api/workflows/{id} endpoint
        endpoints.MapGet("/api/workflows/{id}", async (string id, IPersistenceProvider workflowStore) =>
        {
            var result = await workflowStore.GetWorkflowInstance(id);
            return result != null ? Results.Json(result) : Results.NotFound();
        });

        // Map the POST /api/workflows/{id}/{version} endpoint
        endpoints.MapPost("/api/workflows/{id}/{version?}", async (
            string id, int? version, [FromBody] object data,
            IWorkflowRegistry registry, IWorkflowController workflowService) =>
        {
            var def = registry.GetDefinition(id, version);
            if (def == null)
                return Results.BadRequest($"Workflow definition {id} for version {version} not found");

            string workflowId = null;
            if (data != null && def.DataType != null)
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var dataObj = JsonConvert.DeserializeObject(dataStr, def.DataType);
                workflowId = await workflowService.StartWorkflow(id, version, dataObj);
            }
            else
            {
                workflowId = await workflowService.StartWorkflow(id, version, null);
            }

            return Results.Ok(workflowId);
        });

        // Map the PUT /api/workflows/{id}/suspend endpoint
        endpoints.MapPut("/api/workflows/{id}/suspend", async (string id, IWorkflowController workflowService) =>
        {
            var success = await workflowService.SuspendWorkflow(id);
            return success ? Results.Ok() : Results.BadRequest("Failed to suspend workflow");
        });

        // Map the PUT /api/workflows/{id}/resume endpoint
        endpoints.MapPut("/api/workflows/{id}/resume", async (string id, IWorkflowController workflowService) =>
        {
            var success = await workflowService.ResumeWorkflow(id);
            return success ? Results.Ok() : Results.BadRequest("Failed to resume workflow");
        });

        // Map the DELETE /api/workflows/{id} endpoint
        endpoints.MapDelete("/api/workflows/{id}", async (string id, IWorkflowController workflowService) =>
        {
            var success = await workflowService.TerminateWorkflow(id);
            return success ? Results.Ok() : Results.BadRequest("Failed to terminate workflow");
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
