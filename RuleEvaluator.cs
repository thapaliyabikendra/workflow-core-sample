namespace ACMS.WebApi;

public class RuleEvaluator
{
    private readonly RulesEngine.RulesEngine _rulesEngine;

    public RuleEvaluator(RulesEngine.RulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public async Task EvaluateRulesAsync()
    {
        var context = new UnlockUserContext
        {
            UserId = "user123",
            IsEligibleForUnlock = true
        };

        // Execute all the rules for the given context
        var result = await _rulesEngine.ExecuteAllRulesAsync("UnlockUserEligibility Workflow", context);
        if (result.Any(r => r.IsSuccess))
        {
            // Process success scenario
            Console.WriteLine("User is eligible for unlock.");
        }
        else
        {
            // Process failure scenario
            Console.WriteLine("User is not eligible for unlock.");
        }
    }
}