using System.Dynamic;
using System.Text.Json;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.UnlockUser;

public class UnlockUserApp1Step : StepBodyAsync
{
    public string UserId { get; set; }
    public string AppType { get; set; }
    private readonly RulesEngine.RulesEngine _rulesEngine;

    public UnlockUserApp1Step(RulesEngine.RulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var basicInfo = "{\"name\": \"hello\",\"email\": \"abcy@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyaltyFactor\": 3,\"totalPurchasesToDate\": 10000}";
        dynamic input0 = JsonSerializer.Deserialize<ExpandoObject>(basicInfo);
        var a = input0.name;

        var input1 = new UnlockUserContext { UserId = UserId, IsEligibleForUnlock = true };

        var inputs = new dynamic[]
            {
                    input1
            };
        // Evaluate if the user is eligible for unlocking based on rules
        var result = await _rulesEngine.ExecuteAllRulesAsync("UnlockUserEligibility Workflow", inputs);

        if (!result.Any(r => r.IsSuccess))
        {
            return ExecutionResult.Next();
        }
        return ExecutionResult.Next();

        //// Proceed with unlocking the user in App1
        //using (var client = new HttpClient())
        //{
        //    var response = await client.PostAsJsonAsync("https://api.app1.com/unlock", new { UserId = UserId });

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return ExecutionResult.Next();
        //    }
        //    return ExecutionResult.Next();
        //}
    }
}