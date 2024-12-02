using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class CallBPMApiStep(DynamicHttpClientService dynamicHttpClientService, ILogger<CallBPMApiStep> logger) : StepBodyAsync
{
    public string TaskId { get; set; }  // TaskId passed from the previous step
    public int UserId { get; set; }  // TaskId passed from the previous step
    public string ResponsePrefix { get; set; }  
    public Dictionary<string, object> Response { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Log and simulate calling the BPM API
        Console.WriteLine($"[{TaskId}] Calling BPM API for approval/rejection...");

        // Make the actual call to the BPM API
        Response = await CallBPMApiAsync();
        return ExecutionResult.Next();
    }

    private async Task<Dictionary<string, object>> CallBPMApiAsync()
    {
        // Define your BPM API request configuration
        var requestConfigJson = new JObject
        {
            { "url", "http://localhost:3000/bpm/api/approval-request" },  // Replace with actual BPM API URL
            { "httpMethod", "POST" },
            { "queryParams", new JObject { { "userId", UserId }, { "taskId", TaskId } } },
            { "headers", new JObject { { "Accept", "application/json" } } }
        }.ToString();

        // Create input DTO for the dynamic HTTP client
        var inputDto = new CreateHttpClientDto
        {
            RequestConfig = requestConfigJson
        };

        // Call the dynamic HTTP client service
        var response = await dynamicHttpClientService.CreateHttpClientAsync(inputDto);

        // You can log the response or check status
        //logger.LogInformation($"BPM API Response: {response}");

        return response;
    }
}