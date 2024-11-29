namespace ACMS.WebApi.Workflows.UnlockUser;

public class UnlockUserContext
{
    public string UserId { get; set; }
    public bool IsEligibleForUnlock { get; set; }
}