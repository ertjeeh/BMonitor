using BMonitor.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BMonitor;

public static class BMonitor
{
    public static void AddBMonitorSqlite(this IServiceCollection serviceCollection)
    {
        SharedInitialize(serviceCollection);
        serviceCollection.AddDbContext<BMonitorContext>();
    }

    public static void AddBMonitorSqlServer(this IServiceCollection serviceCollection, string sqlServerConnectionString)
    {
        SharedInitialize(serviceCollection);
        serviceCollection.AddDbContext<BMonitorContext>(o => o.UseSqlServer(sqlServerConnectionString));
    }

    private static void SharedInitialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddControllers();
    }

    public static void UseBMonitor(
        this IApplicationBuilder appBuilder)
    {
        appBuilder.Map("/bmonitor/bmonitor.js", delegate (IApplicationBuilder builder) { Serve(builder, "bmonitor.js"); });
        appBuilder.Map("/bmonitor/bmonitor.css", delegate (IApplicationBuilder builder) { Serve(builder, "bmonitor.css"); });

        appBuilder.Map("/bmonitorstatic/monitors.html", delegate (IApplicationBuilder builder) { Serve(builder, "Static.monitors.html"); });
        appBuilder.Map("/bmonitorstatic/addmonitordialog.html", delegate (IApplicationBuilder builder) { Serve(builder, "Static.addmonitordialog.html"); });

        appBuilder.Map("/bmonitor", delegate (IApplicationBuilder builder) { Serve(builder, "index.html"); });
        appBuilder.Map("/favicon.ico", delegate (IApplicationBuilder builder) { Serve(builder, "favicon.ico", false); });

        ((WebApplication)appBuilder).MapControllers();

        void Serve(IApplicationBuilder builder, string filename, bool htmlContent = true)
        {
            builder.Run(async context =>
            {
                var x = typeof(BMonitor).Assembly.GetManifestResourceNames();
                if (htmlContent)
                {
                    var s = typeof(BMonitor).Assembly.GetManifestResourceStream($"BMonitor.{filename}");
                    if (s == null)
                    {
                        return;
                    }
                    using var stream = new StreamReader(s);
                    var html = await stream.ReadToEndAsync();
                    await context.Response.WriteAsync(html, Encoding.UTF8);
                }
                else
                {
                    await using var stream = typeof(BMonitor).Assembly.GetManifestResourceStream($"BMonitor.{filename}");
                    if (stream == null)
                    {
                        return;
                    }
                    await using var memStream = new MemoryStream();
                    await stream.CopyToAsync(memStream);
                    await context.Response.BodyWriter.WriteAsync(memStream.ToArray());
                }
            });
        }
    }
};