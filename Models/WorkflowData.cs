using Newtonsoft.Json.Linq;

namespace ACMS.WebApi.Models;

public record WorkflowData
{
    public JObject ApiResponse { get; set; }
}