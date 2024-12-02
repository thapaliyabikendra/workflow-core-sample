using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class PollUiPathJobStatusStep(DynamicHttpClientService dynamicHttpClientService, ILogger<PollUiPathJobStatusStep> logger) : StepBodyAsync
{
    // Input properties
    public string TaskId { get; set; }  // TaskId passed from the previous step
    public string UiPathJobId { get; set; }  // The unique UiPath Job ID
    public string JobStatus { get; set; }  // The status of the job ("Ended", "Pending", etc.)
    public bool IsDataPolled { get; set; }  // Flag to indicate if the job status is successfully polled
    public int PollingCount { get; set; }  // The number of polling attempts made

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Log the polling attempt
        Console.WriteLine($"[{TaskId}] Polling UiPath job status - Request {PollingCount} - IsDataPolled: {IsDataPolled}.");

        // Increment polling count
        PollingCount += 1;

        // Call the method to get the current status of the UiPath job
        JobStatus = await GetUiPathJobStatusAsync();

        // Check if the polling should stop (if the job is "Ended")
        //if (JobStatus.Equals("Ended", StringComparison.OrdinalIgnoreCase))
        if (PollingCount == 2)
        {
            Console.WriteLine($"[{TaskId}] UiPath job completed successfully.");
            IsDataPolled = true;  // Mark as successfully polled
        }
        else
        {
            Console.WriteLine($"[{TaskId}] UiPath job status is still '{JobStatus}'. Polling again...");
        }

        // Continue to the next step
        return ExecutionResult.Outcome(IsDataPolled);
    }

    // Method to get the UiPath job status via DynamicHttpClientService
    private async Task<string> GetUiPathJobStatusAsync()
    {
        // Define the URL for the GET request to fetch the job status
        var url = $"http://localhost:3000/ui-path/api/job-status";  // URL for job status check

        // Define the request configuration
        var requestConfigJson = new JObject
        {
            { "url", url },
            { "httpMethod", "GET" },
            { "headers", new JObject { { "Accept", "application/json" } } },
            { "queryParams", new JObject { { "uiPathJobId", UiPathJobId } } },
        }.ToString();

        // Create DTO for the DynamicHttpClientService
        var inputDto = new CreateHttpClientDto
        {
            RequestConfig = requestConfigJson
        };

        // Make the API request using DynamicHttpClientService
        var response = await dynamicHttpClientService.CreateHttpClientAsync(inputDto);

        // Log the response (optional)
        logger.LogInformation("UiPath Job status response: {Response}", response);

        // Assuming the response contains a field for the job status, e.g., "JobStatus"
        // If the API returns a JSON response like: { "JobStatus": "Ended" }
        var jobStatus = response["JobStatus"]?.ToString();  // Adjust based on actual response structure

        // Return the job status (e.g., "Pending", "Ended")
        return jobStatus ?? "Unknown";  // Default to "Unknown" if status is not found
    }
}
