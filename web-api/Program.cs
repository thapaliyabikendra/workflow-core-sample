using ACMS.WebApi.EntityFrameworkCore;
using ACMS.WebApi.Extensions;
using ACMS.WebApi.Middlewares;
using ACMS.WebApi.Services;
using ACMS.WebApi.Utilities;
using ACMS.WebApi.Workflows.Transfers.Steps;
using ACMS.WebApi.Workflows.UnlockUser;
using Microsoft.EntityFrameworkCore;
using Nest;
using Serilog;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.Extensions.Logging;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
#if DEBUG
            //.MinimumLevel.Debug()
#else
#endif
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("WorkflowCore.Services.BackgroundTasks.IndexConsumer", LogEventLevel.Warning)
            .MinimumLevel.Override("ACMS.WebApi.Services.DynamicHttpClientService", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.HttpRequestIn", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.HttpRequestOut", LogEventLevel.Warning)
            //.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] WorkflowId={@WorkflowId} StepId={@StepId}: {Message} {NewLine}{Exception}")
            .Enrich.FromLogContext() // To include log scope data
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var configuration = builder.Configuration;
//var sqliteConnectionString = @"Data Source=employees.db;";
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddWorkflow(cfg =>
{
    //cfg.UseSqlite(sqliteConnectionString, true);
    cfg.UsePostgreSQL(configuration.GetConnectionString("Default"), true, true);
    var esUri = builder.Configuration["Elasticsearch:Uri"];
    var esIndex = builder.Configuration["Elasticsearch:IndexName"];
    cfg.UseElasticsearch(new ConnectionSettings(new Uri(esUri)), esIndex);
});
builder.Services.AddWorkflowDSL();  // Register WorkflowCore.DSL
builder.Services.AddWorkflowStepMiddleware<LogCorrelationStepMiddleware>();

// Configure RulesEngine (with rule loading service)
builder.Services.AddSingleton<DynamicHttpClientService>();
builder.Services.AddSingleton<RuleService>();
builder.Services.AddSingleton<CallApiStep>();
builder.Services.AddSingleton<PrintMessageStep>();
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
//builder.Services
//    .AddDbContext<EmployeeContext>(options =>
//        options.UseSqlite(sqliteConnectionString));
builder.Services.AddDbContext<EmployeeContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("Default")));

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
