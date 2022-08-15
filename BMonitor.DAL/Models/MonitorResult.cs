using System.ComponentModel.DataAnnotations;

namespace BMonitor.DAL.Models;

public class MonitorResult
{
    [Key]
    public long Id { get; set; }

    public int MonitorId { get; set; }
    public DateTime DateTime { get; set; }

    public StatusResultId StatusResultId { get; set; }
    public StatusResult StatusResult { get; set; }
    public int? IntResult { get; set; }
    public string? StringResult { get; set; }

    public virtual Monitor Monitor { get; set; }
}