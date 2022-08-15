namespace BMonitor.DAL.Models.Monitors;

public class FolderMonitor : Monitor
{
    public string Path { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }

    public override string GetSubType()
    {
        return nameof(FolderMonitor);
    }
}