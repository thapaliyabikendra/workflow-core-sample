using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.Transfers.Steps;

public class CallApiStep(DynamicHttpClientService httpClientService, ILogger<CallApiStep> logger) : StepBodyAsync
{
    private readonly ILogger _logger = logger;

    public string RequestConfigJson { get; set; }
    public JObject Response { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        try
        {
            var inputDto = new CreateHttpClientDto
            {
                RequestConfig = RequestConfigJson
            };

            // Make the API call asynchronously
            Response = await httpClientService.CreateHttpClientAsync(inputDto);

            // If the response is null, log an error and throw an exception
            if (Response == null)
            {
                _logger.LogError("API call failed");
                throw new Exception("API call failed");
            }

            // Return the next step in the workflow
            return ExecutionResult.Next();
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow if you want to fail the workflow
            _logger.LogError(ex, "An error occurred in the CallApiStep.");
            throw; // This will cause the workflow to fail
        }
    }
}
