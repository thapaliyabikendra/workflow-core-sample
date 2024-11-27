using Newtonsoft.Json;
using RulesEngine.Models;

namespace ACMS.WebApi;

public class RuleService
{
    private readonly RulesEngine.RulesEngine _rulesEngine;

    public RuleService(IConfiguration configuration)
    {
        // Load the rules from JSON file
        var workflows = LoadRulesFromJson("rules.json");

        // Initialize RulesEngine with workflows
        _rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray());
    }

    public RulesEngine.RulesEngine GetRulesEngine() => _rulesEngine;

    // Method to load rules from JSON file
    private List<Workflow> LoadRulesFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);

        // Deserialize the JSON into workflows
        var workflows = JsonConvert.DeserializeObject<List<Workflow>>(json);

        // Return the workflows containing rules
        return workflows;
    }
}