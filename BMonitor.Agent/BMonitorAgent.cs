using BMonitor.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMonitor.Agent;

public static class BMonitorAgent
{
    public static void AddBMonitorAgentSqlite(this IServiceCollection serviceCollection)
    {
        SharedInitializeStart(serviceCollection);
        serviceCollection.AddDbContext<BMonitorContext>();
        /*
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BMonitor",
                        "db.sqllitedb");
        if (!Directory.Exists(Path.GetDirectoryName(dbPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? string.Empty);
        }

        optionsBuilder.UseSqlite(
            $"Data Source={dbPath}")

        */
        SharedInitializeEnd(serviceCollection);
    }

    public static void AddBMonitorAgentSqlServer(this IServiceCollection serviceCollection, string sqlServerConnectionString)
    {
        SharedInitializeStart(serviceCollection);
        serviceCollection.AddDbContext<BMonitorContext>(o => o.UseSqlServer(sqlServerConnectionString));
        SharedInitializeEnd(serviceCollection);
    }

    private static void SharedInitializeStart(IServiceCollection serviceCollection)
    {
        serviceCollection.AddLogging();
    }

    private static void SharedInitializeEnd(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<MonitorService>();
    }

    public static void RunBMonitorAgent(this IServiceProvider serviceProvider)
    {
    }
}