using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class TriggerUiPathJobStep(DynamicHttpClientService dynamicHttpClientService, ILogger<TriggerUiPathJobStep> logger) : StepBodyAsync
{

    // Input properties
    public string TaskId { get; set; }  // TaskId passed from the previous step
    public string UiPathJobId { get; set; }  // The unique JobId for the UiPath job

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Log and simulate triggering the UiPath job
        Console.WriteLine($"[{TaskId}] Triggering UiPath job...");

        // Trigger UiPath job and get the Job ID
        UiPathJobId = await TriggerUiPathJobAsync();

        // Log the UiPath Job ID that was triggered
        Console.WriteLine($"Job triggered. UiPath Job ID: {UiPathJobId}");

        // Return the result to the next step in the workflow
        return ExecutionResult.Next();
    }

    // Method to trigger the UiPath job via the DynamicHttpClientService
    private async Task<string> TriggerUiPathJobAsync()
    {
        // Define the URL for triggering the UiPath job (replace with your Orchestrator API endpoint)
        string url = $"http://localhost:3000/ui-path/api/start-job";  // URL to trigger UiPath job

        // Define the request configuration
        var requestConfigJson = new JObject
        {
            { "url", url },
            { "httpMethod", "POST" },
            { "headers", new JObject { { "Accept", "application/json" } } },
            { "queryParams", new JObject { { "taskId", TaskId } } },
        }.ToString();

        // Create DTO to pass into the DynamicHttpClientService
        var inputDto = new CreateHttpClientDto
        {
            RequestConfig = requestConfigJson
        };

        // Call the DynamicHttpClientService to make the HTTP request
        var response = await dynamicHttpClientService.CreateHttpClientAsync(inputDto);

        // Log the response (optional)
        logger.LogInformation("UiPath Job trigger response: {response}", response);

        // Parse the response to extract the Job ID (assuming response contains JobId or similar field)
        var jobId = response["UiPathJobId"]?.ToString();  // Example: Adjust based on actual response structure

        // If Job ID is not found, throw an error (or handle gracefully)
        if (string.IsNullOrEmpty(jobId))
        {
            throw new Exception("Failed to retrieve UiPath Job ID.");
        }

        return jobId;  // Return the UiPath Job ID to use in subsequent steps
    }
}
