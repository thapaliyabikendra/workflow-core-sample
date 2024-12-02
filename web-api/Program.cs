using ACMS.WebApi.EntityFrameworkCore;
using ACMS.WebApi.Extensions;
using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using ACMS.WebApi.Utilities;
using ACMS.WebApi.Workflows.Transfers;
using ACMS.WebApi.Workflows.Transfers.Steps;
using ACMS.WebApi.Workflows.UnlockUser;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Linq.Dynamic.Core;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);
var sqliteConnectionString = @"Data Source=employees.db;";
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddWorkflow(cfg =>
{
    //cfg.UseSqlite(sqliteConnectionString, true);
    cfg.UseElasticsearch(new ConnectionSettings(new Uri("http://localhost:9200")), "ACMS_Workflows_index");
});
builder.Services.AddWorkflowDSL();  // Register WorkflowCore.DSL

// Configure RulesEngine (with rule loading service)
builder.Services.AddSingleton<DynamicHttpClientService>();
builder.Services.AddSingleton<RuleService>();
builder.Services.AddSingleton<CallApiStep>();
builder.Services.AddTransient<CallBPMApiStep>();
builder.Services.AddTransient<TriggerUiPathJobStep>();
builder.Services.AddTransient<PollUiPathJobStatusStep>();
builder.Services.AddTransient<UnlockUserApp1Step>();
builder.Services.AddTransient<UnlockUserApp2Step>();
builder.Services.AddSingleton(serviceProvider =>
{
    var ruleService = serviceProvider.GetRequiredService<RuleService>();
    return ruleService.GetRulesEngine();
});
// Register the EmployeeContext and configure SQLite
builder.Services
    .AddDbContext<EmployeeContext>(options =>
        options.UseSqlite(sqliteConnectionString));

builder.Host.ConfigureLogging((context, logging) =>
  {
      logging.ClearProviders();
      logging.AddConsole(); // or any other logging provider
      logging.SetMinimumLevel(LogLevel.Information); // default level for all logs

      logging.AddFilter("System.Net.Http", LogLevel.Warning); // Set System.Net.Http logs to Warning level
      logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
      logging.AddFilter("WorkflowCore.Services.BackgroundTasks.IndexConsumer", LogLevel.Error);
      logging.AddFilter("ACMS.WebApi.Services.DynamicHttpClientService", LogLevel.Error); 
  });
var app = builder.Build();

// Ensure that the database schema is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
    await dbContext.Database.MigrateAsync();  // Apply migrations if necessary

    DataSeeder.Seed(dbContext);
}

//var workflowJson = File.ReadAllText("Workflows/UnlockUser/workflow.json");  // Path to your workflow JSON file
var workflowJson = File.ReadAllText("Workflows/Transfers/EmployeeTransferWorkflow.json");
//var workflowJson = File.ReadAllText("Workflows/Transfers/EmployeeTransferWorkflowWithDynamicData.json");
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
//workflowHost.RegisterWorkflow<EmployeeTransferWorkflow, EmployeeTransferDataDto>();
//workflowHost.RegisterWorkflow<EmployeeTransferWorkflowWithDynamicData, DynamicData>();
await workflowHost.StartAsync(default);
await app.RunAsync();
