namespace ACMS.WebApi;

public class UnlockUserContext
{
    public string UserId { get; set; }
    public bool IsEligibleForUnlock { get; set; }
}