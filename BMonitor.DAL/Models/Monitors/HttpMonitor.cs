namespace BMonitor.DAL.Models.Monitors;

public class HttpMonitor : Monitor
{
    public string Endpoint { get; set; }
    public int ExpectedStatusCode { get; set; }
    public int TimeoutInMs { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }

    public override string GetSubType()
    {
        return nameof(HttpMonitor);
    }
}