using ACMS.WebApi.Services;
using ACMS.WebApi.Workflows.UnlockUser;
using Microsoft.Extensions.Hosting;
using System.Dynamic;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddWorkflow();
builder.Services.AddWorkflowDSL();  // Register WorkflowCore.DSL

// Configure RulesEngine (with rule loading service)
builder.Services.AddSingleton<RuleService>();
builder.Services.AddTransient<UnlockUserApp1Step>();
builder.Services.AddTransient<UnlockUserApp2Step>();
builder.Services.AddSingleton(serviceProvider =>
{
    var ruleService = serviceProvider.GetRequiredService<RuleService>();
    return ruleService.GetRulesEngine();
});

var app = builder.Build();

var workflowJson = File.ReadAllText("Workflows/UnlockUser/workflow.json");  // Path to your workflow JSON file
var loader = app.Services.GetRequiredService<IDefinitionLoader>();

// Load the workflow definition from the JSON string
loader.LoadDefinition(workflowJson, Deserializers.Json);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapPost("/api/unlock", async (UnlockRequest request, IWorkflowHost workflowHost, IServiceProvider serviceProvider) =>
{
    // Prepare workflow data
    var workflowData = new Dictionary<string, object>
                {
                    { "UserId", request.UserId }
                };

    // Start the workflow with input data
    await workflowHost.StartWorkflow("UnlockUserWorkflow", workflowData);  // "UnlockUserWorkflow" is the workflow Id
    return Results.Ok("Workflow started.");
});
var workflowHost = app.Services.GetService<IWorkflowHost>();
await workflowHost.StartAsync(default);
await app.RunAsync();
