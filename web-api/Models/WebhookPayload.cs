namespace ACMS.WebApi.Models;

public record WebhookPayload
{
    public string TaskId { get; set; }
    public string ApprovalStatus { get; set; } // "Approved" or "Rejected"
}