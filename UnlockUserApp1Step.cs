using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi;

public class UnlockUserApp1Step : StepBodyAsync
{
    public string UserId { get; set; }
    private readonly RulesEngine.RulesEngine _rulesEngine;

    public UnlockUserApp1Step(RulesEngine.RulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var unlockUserContext = new UnlockUserContext { UserId = UserId, IsEligibleForUnlock = true };

        // Evaluate if the user is eligible for unlocking based on rules
        var result = await _rulesEngine.ExecuteAllRulesAsync("UnlockUserEligibility Workflow", context);

        if (!result.Any(r => r.IsSuccess))
        {
            return ExecutionResult.Next();
        }

        // Proceed with unlocking the user in App1
        using (var client = new HttpClient())
        {
            var response = await client.PostAsJsonAsync("https://api.app1.com/unlock", new { UserId = UserId });

            if (response.IsSuccessStatusCode)
            {
                return ExecutionResult.Next();
            }
            return ExecutionResult.Next();
        }
    }
}