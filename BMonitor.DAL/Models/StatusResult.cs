using System.ComponentModel.DataAnnotations;

namespace BMonitor.DAL.Models;

public class StatusResult
{
    [Key]
    public StatusResultId StatusResultId { get; set; }

    public string Name { get; set; }
}

public enum StatusResultId
{
    Succeeded = 0,
    Failed = -1,
    Unknown = 1
}