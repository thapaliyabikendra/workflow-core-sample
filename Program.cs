using ACMS.WebApi;
using Microsoft.Extensions.Hosting;
using System.Dynamic;
using WorkflowCore.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddWorkflow();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapPost("/api/unlock", async (UnlockRequest request, IWorkflowHost workflowHost, RuleService ruleService) =>
{
    // Load the rules engine
    var rulesEngine = ruleService.GetRulesEngine();

    // Evaluate rules (example)
    var evaluator = new RuleEvaluator(rulesEngine);
    await evaluator.EvaluateRulesAsync();

    // Start the workflow
    var workflowData = new Dictionary<string, object>
    {
        { "UserId", request.UserId }
    };
    await workflowHost.StartWorkflow("UnlockUserWorkflow", workflowData);
    //await workflowHost.StopAsync(default);
    return Results.Ok("Workflow started for user unlock.");
});

var workflowHost = app.Services.GetService<IWorkflowHost>();
workflowHost.RegisterWorkflow<UnlockUserWorkflow>();
await workflowHost.StartAsync(default);

await app.RunAsync();
