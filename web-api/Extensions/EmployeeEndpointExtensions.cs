// Extensions/EmployeeEndpointExtensions.cs
using ACMS.WebApi.Entities;
using ACMS.WebApi.EntityFrameworkCore;
using ACMS.WebApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;

namespace ACMS.WebApi.Extensions;

public static class EmployeeEndpointExtensions
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Register the GET, POST, PUT, DELETE endpoints for employee management

        // Get all employees
        endpoints.MapGet("/api/employees", async (EmployeeContext context) =>
        {
            var employees = await context.Employees.ToListAsync();
            return Results.Ok(employees);
        });

        // Get an employee by ID
        endpoints.MapGet("/api/employees/{id}", async (int id, EmployeeContext context) =>
        {
            var employee = await context.Employees.FindAsync(id);
            if (employee == null)
            {
                return Results.NotFound($"Employee with ID {id} not found.");
            }
            return Results.Ok(employee);
        });

        // Add a new employee
        endpoints.MapPost("/api/employees", async (Employee employee, EmployeeContext context) =>
        {
            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            return Results.Created($"/api/employees/{employee.Id}", employee);
        });

        // Transfer employee from one branch to another
        endpoints.MapPost("/api/employees/{id}/transfer", async (int id, string newBranch, EmployeeContext context, IWorkflowHost workflowHost, ILogger<IEndpointRouteBuilder> logger) =>
        {
            var employee = await context.Employees.FindAsync(id);
            if (employee == null)
            {
                return Results.NotFound($"Employee with ID {id} not found.");
            }

            // Prepare the data for the workflow
            //var workflowData = new EmployeeTransferDataDto
            //{
            //    UserId = id,
            //    FromBranch = employee.Branch,
            //    ToBranch = newBranch,
            //};
            //// Start the workflow
            //await workflowHost.StartWorkflow("EmployeeTransferWorkflow", workflowData);

            var initialData = new DynamicData
            {
                //["TaskId"] = Guid.Empty,
                ["UserId"] = id,
                //["IsDataPolled"] = false,
                //["PollingCount"] = 0,
                //["UiPathJobId"] = string.Empty,
                ["FromBranch"] = employee.Branch,
                ["ToBranch"] = newBranch
            };
           
            var workflowId = await workflowHost.StartWorkflow("EmployeeTransferWorkflow", initialData);

            var msg = $"Workflow Id : {workflowId} - Workflow started.";
            logger.LogInformation(msg);
            return Results.Ok(msg);

            //// Transfer the employee to the new branch
            //employee.Branch = newBranch;
            //await context.SaveChangesAsync();

            //return Results.Ok(employee); // Return updated employee with new branch
        });

        // Delete an employee
        endpoints.MapDelete("/api/employees/{id}", async (int id, EmployeeContext context) =>
        {
            var employee = await context.Employees.FindAsync(id);
            if (employee == null)
            {
                return Results.NotFound($"Employee with ID {id} not found.");
            }

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();

            return Results.NoContent();
        });

        endpoints.MapPost("/api/employees/unlock", async (UnlockRequest request, IWorkflowHost workflowHost) =>
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return Results.BadRequest("UserId is required.");
            }

            // Prepare the data for the workflow
            var workflowData = new Dictionary<string, object>
                {
                    { "UserId", request.UserId }
                };

            // Start the workflow
            await workflowHost.StartWorkflow("UnlockUserWorkflow", workflowData);

            return Results.Ok("Workflow started.");
        });

        return endpoints;
    }
}