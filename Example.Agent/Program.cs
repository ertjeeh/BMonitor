// See https://aka.ms/new-console-template for more information

using BMonitor.Agent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");
Console.WriteLine("Press ctrl-c to exit");
var host = CreateHostBuilder(args).Build();

host.Run();

static IHostBuilder CreateHostBuilder(string[] args)
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, builder) =>
        {
            builder.SetBasePath(Directory.GetCurrentDirectory());
        })
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(l =>
            {
                l.SetMinimumLevel(LogLevel.Debug);
                l.AddDebug();
                l.AddConsole();
                l.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            });
            services.AddBMonitorAgentSqlServer("Server=127.0.0.1,14330;Database=BMonitor;User Id=sa;Password=*******;TrustServerCertificate=True");
        })
        .UseConsoleLifetime();

    return hostBuilder;
}