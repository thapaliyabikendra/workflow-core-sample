using WorkflowCore.Interface;

namespace ACMS.WebApi;

public class UnlockUserWorkflow : IWorkflow
{
    public string Id => "UnlockUserWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
           .StartWith<UnlockUserApp1Step>()
           .Then<UnlockUserApp2Step>();
    }
}