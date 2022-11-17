using BMonitor.DAL;
using BMonitor.DAL.Models;
using BMonitor.DAL.Models.Monitors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Monitor = BMonitor.DAL.Models.Monitor;

namespace BMonitor.Agent;

public class MonitorService : IHostedService
{
    private readonly ILogger<MonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Task _mainLoop;
    private bool _stopRequested;

    public MonitorService(ILogger<MonitorService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _stopRequested = false;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MonitorService starting..");
        _logger.LogInformation("MonitorService started.");
        _mainLoop = MainLoop();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stopRequested = true;

        // wait for main loop to complete
        throw new NotImplementedException();
    }

    private async Task MainLoop()
    {
        _logger.LogInformation($"MainLoop started.");
        while (!_stopRequested)
        {
            try
            {
                // remove
                await Task.Delay(2500);

                var monitorsToUpdate = await GetMonitorsToUpdate();
                if (!monitorsToUpdate.Any())
                {
                    await Task.Delay(10);
                    continue;
                }

                var tasks = GetUpdateTasks(monitorsToUpdate);

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occured: {e}");
            }
        }
    }

    private async Task<IList<Monitor>> GetMonitorsToUpdate()
    {
        using var scope = _serviceProvider.CreateScope();
        var mc = scope.ServiceProvider.GetRequiredService<BMonitorContext>();
        return await mc.Monitors.AsNoTracking().Where(m =>
                m.LastUpdate == null || m.LastUpdate < DateTime.UtcNow.AddMilliseconds(-m.UpdateIntervalInMs))
            .ToListAsync();
    }

    private IEnumerable<Task> GetUpdateTasks(IEnumerable<Monitor> monitors)
    {
        return monitors.Select(UpdateMonitor);
    }

    private async Task UpdateMonitor(Monitor monitor)
    {
        if (monitor.GetType() == typeof(PingMonitor))
        {
            await UpdatePingMonitor((PingMonitor)monitor);
        }

        if (monitor.GetType() == typeof(FolderMonitor))
        {
            await UpdateFolderMonitor((FolderMonitor)monitor);
        }

        if (monitor.GetType() == typeof(HttpMonitor))
        {
            await UpdateHttpMonitor((HttpMonitor)monitor);
        }

        if (monitor.GetType() == typeof(SqlMonitor))
        {
            await UpdateSqlMonitor((SqlMonitor)monitor);
        }
    }

    private async Task UpdateSqlMonitor(SqlMonitor monitor)
    {
        using var scope = _serviceProvider.CreateScope();
        await using var mc = scope.ServiceProvider.GetRequiredService<BMonitorContext>();

        try
        {
            var result = await ExecuteSql(monitor);
            var dbMonitor = await mc.Monitors.SingleAsync(m => m.Id == monitor.Id);
            dbMonitor.LastUpdate = CurrentUtcDateTime();
            mc.MonitorResults.Add(new MonitorResult
            {
                Monitor = dbMonitor,
                DateTime = CurrentUtcDateTime(),
                IntResult = result,
                StatusResultId = StatusResultId.Succeeded,
            });
            _logger.LogDebug("For monitor {Monitor}, Sql result: {Result}.", monitor, result);

            await mc.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "For monitor {Monitor}, exception happened, message: {Msg}", monitor, e.Message);
        }
    }

    private async Task<int> ExecuteSql(SqlMonitor monitor)
    {
        await using var conn = new SqlConnection(monitor.ConnectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = monitor.Query;
        var result = (int)await cmd.ExecuteScalarAsync();
        await conn.CloseAsync();
        return result;
    }

    private async Task UpdateHttpMonitor(HttpMonitor monitor)
    {
        using var scope = _serviceProvider.CreateScope();
        await using var mc = scope.ServiceProvider.GetRequiredService<BMonitorContext>();

        try
        {
            var (status, ms, content) = await CheckHttpEndpoint(monitor);
            var dbMonitor = await mc.Monitors.SingleAsync(m => m.Id == monitor.Id);
            dbMonitor.LastUpdate = CurrentUtcDateTime();
            mc.MonitorResults.Add(new MonitorResult
            {
                Monitor = dbMonitor,
                DateTime = CurrentUtcDateTime(),
                IntResult = ms,
                StatusResultId = status,
                StringResult = content
            });
            _logger.LogDebug("For monitor {Monitor}, Expected HttpStatusCode: {Code}, Result {Result}, in {Ms}ms.",
                monitor, monitor.ExpectedStatusCode, status, ms);

            await mc.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "For monitor {Monitor}, exception happened, message: {Msg}", monitor, e.Message);
        }
    }

    private async Task<(StatusResultId status, int ms, string content)> CheckHttpEndpoint(HttpMonitor monitor)
    {
        using var httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true }) { Timeout = TimeSpan.FromMilliseconds(monitor.TimeoutInMs) };
        var sw = new Stopwatch();
        sw.Start();
        var response = await httpClient.GetAsync(monitor.Endpoint);
        sw.Stop();
        return (monitor.ExpectedStatusCode == (int)response.StatusCode
                ? StatusResultId.Succeeded
                : StatusResultId.Failed,
            (int)sw.ElapsedMilliseconds,
            await response.Content.ReadAsStringAsync());
    }

    private async Task UpdateFolderMonitor(FolderMonitor monitor)
    {
        var path = monitor.Path;

        using var scope = _serviceProvider.CreateScope();
        await using var mc = scope.ServiceProvider.GetRequiredService<BMonitorContext>();

        var result = -1;
        try
        {
            result = GetItemsInFolder(path);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "For monitor {Monitor}, exception happened, message: {Msg}", monitor, e.Message);
        }

        try
        {
            var dbMonitor = await mc.Monitors.SingleAsync(m => m.Id == monitor.Id);
            dbMonitor.LastUpdate = CurrentUtcDateTime();
            mc.MonitorResults.Add(new MonitorResult
            {
                Monitor = dbMonitor,
                DateTime = CurrentUtcDateTime(),
                IntResult = result,
                StatusResultId = result < 0 ? StatusResultId.Failed : StatusResultId.Succeeded,
            });
            _logger.LogDebug("For monitor {Monitor}, got folder count: {Count}", monitor, result);

            await mc.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "For monitor {Monitor}, saving exception happened, message: {Msg}", monitor, e.Message);
        }
    }

    private static int GetItemsInFolder(string path)
    {
        return !Directory.Exists(path)
            ? -1
            : Directory.GetFileSystemEntries(path).Length;
    }

    private async Task UpdatePingMonitor(PingMonitor monitor)
    {
        var bytes = new byte[32];
        new Random(unchecked((int)DateTime.Now.Ticks)).NextBytes(bytes);
        var reply = new Ping().Send(monitor.Endpoint, monitor.TimeoutInMs, bytes);
        _logger.LogDebug("For monitor {Monitor}, got ping reply: {reply.RoundtripTime}ms, status: {reply.Status}",
            monitor, reply.RoundtripTime.ToString(), reply.Status.ToString());

        using var scope = _serviceProvider.CreateScope();
        await using var mc = scope.ServiceProvider.GetRequiredService<BMonitorContext>();
        try
        {
            var dbMonitor = await mc.Monitors.SingleAsync(m => m.Id == monitor.Id);
            dbMonitor.LastUpdate = CurrentUtcDateTime();
            mc.MonitorResults.Add(new MonitorResult
            {
                Monitor = dbMonitor,
                DateTime = CurrentUtcDateTime(),
                IntResult = reply.Status == IPStatus.Success
                    ? reply.RoundtripTime < int.MaxValue ? Convert.ToInt32(reply.RoundtripTime) : int.MaxValue
                    : monitor.TimeoutInMs + 1,
                StatusResultId = reply.Status switch
                {
                    IPStatus.Success => StatusResultId.Succeeded,
                    IPStatus.Unknown => StatusResultId.Unknown,
                    _ => StatusResultId.Failed
                }
            });
            await mc.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Something weird happened while saving: {E}", e);
        }
    }

    private static DateTime CurrentUtcDateTime() => DateTime.UtcNow;
}