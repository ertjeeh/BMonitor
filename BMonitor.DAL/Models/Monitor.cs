using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMonitor.DAL.Models;

public abstract class Monitor
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public int UpdateIntervalInMs { get; set; }
    public DateTime? LastUpdate { get; set; }

    public virtual List<MonitorResult> MonitorResults { get; set; }

    public override string ToString() => Name;

    [NotMapped]
    public string MonitorType
    {
        get
        {
            return GetSubType();
        }
    }

    public virtual string GetSubType()
    {
        return "BaseType";
    }
}