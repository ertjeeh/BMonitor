namespace BMonitor.DAL.Models.Monitors;

public class SqlMonitor : Monitor
{
    public string ConnectionString { get; set; }
    public string Query { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }

    public override string GetSubType()
    {
        return nameof(SqlMonitor);
    }
}