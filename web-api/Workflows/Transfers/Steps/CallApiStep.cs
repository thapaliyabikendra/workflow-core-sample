using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using ExecutionResult = WorkflowCore.Models.ExecutionResult;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class CallApiStep(DynamicHttpClientService dynamicHttpClientService, ILogger<CallApiStep> logger) : StepBodyAsync
{
    public JObject Data { get; set; }
    public JObject Response { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        try
        {
            Response = await CallApiAsync();
            // Return the next step in the workflow
            return ExecutionResult.Next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in the CallApiStep.");
            throw;
        }
    }

    private async Task<JObject> CallApiAsync()
    {
        // Define your BPM API request configuration
        var requestConfigJson = new JObject
        {
            { "url", (string)Data["Url"]  },  // Replace with actual BPM API URL
            { "httpMethod", "GET" },
            { "queryParams", new JObject { { "userId", (int)Data["UserId"] }, { "taskId", (string)Data["TaskId"] }, { "uiPathJobId", (string)Data["UiPathJobId"] } } },
            { "headers", new JObject { { "Accept", "application/json" } } }
        }.ToString();

        // Create input DTO for the dynamic HTTP client
        var inputDto = new CreateHttpClientDto
        {
            RequestConfig = requestConfigJson
        };

        // Call the dynamic HTTP client service
        var response = await dynamicHttpClientService.CreateHttpClientAsync(inputDto);
        return response; 
    }
}
