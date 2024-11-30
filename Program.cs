using ACMS.WebApi.EntityFrameworkCore;
using ACMS.WebApi.Extensions;
using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using ACMS.WebApi.Utilities;
using ACMS.WebApi.Workflows.Transfers;
using ACMS.WebApi.Workflows.UnlockUser;
using Microsoft.EntityFrameworkCore;
using Nest;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddWorkflow(cfg =>
{
	cfg.UseElasticsearch(new ConnectionSettings(new Uri("http://localhost:9200")), "ACMS_Workflows_index");
});
builder.Services.AddWorkflowDSL();  // Register WorkflowCore.DSL

// Configure RulesEngine (with rule loading service)
builder.Services.AddSingleton<DynamicHttpClientService>();
builder.Services.AddSingleton<RuleService>();
builder.Services.AddTransient<UnlockUserApp1Step>();
builder.Services.AddTransient<UnlockUserApp2Step>();
builder.Services.AddSingleton(serviceProvider =>
{
    var ruleService = serviceProvider.GetRequiredService<RuleService>();
    return ruleService.GetRulesEngine();
});
// Register the EmployeeContext and configure SQLite
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlite("Data Source=employees.db"));



var app = builder.Build();

// Ensure that the database schema is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
    await dbContext.Database.MigrateAsync();  // Apply migrations if necessary

    DataSeeder.Seed(dbContext);
}

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

app.MapWorkflowEndpoints();
app.MapEmployeeEndpoints();
app.MapWebhookEndpoints();  // This maps the /api/webhook endpoint

var workflowHost = app.Services.GetService<IWorkflowHost>();
workflowHost.RegisterWorkflow<EmployeeTransferWorkflow, EmployeeTransferDataDto>();
await workflowHost.StartAsync(default);
await app.RunAsync();
