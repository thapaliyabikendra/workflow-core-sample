namespace ACMS.WebApi.Models;

public class EmployeeTransferDataDto
{
    public int UserId { get; set; }
    public string FromBranch { get; set; }
    public string ToBranch { get; set; }
    public string TaskId { get; set; }
    public string ApprovalStatus { get; set; }
    public int PollingCount { get; set; }
}
