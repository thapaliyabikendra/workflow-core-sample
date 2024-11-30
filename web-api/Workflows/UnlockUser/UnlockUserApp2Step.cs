using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ACMS.WebApi.Workflows.UnlockUser;

public class UnlockUserApp2Step : StepBody
{
    public string UserId { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        // Unlock user in App2
        using (var client = new HttpClient())
        {
            var response = client.PostAsJsonAsync("https://api.app2.com/unlock", new { UserId }).Result;

            if (response.IsSuccessStatusCode)
            {
                return ExecutionResult.Next();
            }
            return ExecutionResult.Next();
        }
    }
}