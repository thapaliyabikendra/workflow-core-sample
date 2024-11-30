using Newtonsoft.Json.Linq;

namespace ACMS.WebApi.Models;

public class EmployeeTransferDataDto
{
    public int UserId { get; set; }
    public string FromBranch { get; set; }
    public string ToBranch { get; set; }
    public string TaskId { get; set; }
    public string ApprovalStatus { get; set; }
    public bool IsDataPolled { get; set; }
    public int PollingCount { get; set; }
    public string UiPathJobId { get; set; }
    public JObject ApiResponse1 { get; set; } 
    public JObject ApiResponse2 { get; set; } 
    public JObject ApiResponse3 { get; set; }
}
