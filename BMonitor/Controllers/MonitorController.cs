using BMonitor.Controllers.Models;
using BMonitor.DAL;
using BMonitor.DAL.Models;
using BMonitor.DAL.Models.Monitors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMonitor.Controllers;

[ApiController]
[Route("/bmonitorapi/[controller]")]
public class MonitorController
{
    private readonly ILogger<MonitorController> _logger;
    private readonly BMonitorContext _bMonitorContext;

    public MonitorController(ILogger<MonitorController> logger, BMonitorContext bMonitorContext)
    {
        _logger = logger;
        _bMonitorContext = bMonitorContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var monitors = await _bMonitorContext.Monitors
            .ToListAsync();
        return new OkObjectResult(monitors);
    }

    [HttpGet, Route("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var monitor = await _bMonitorContext.Monitors.SingleOrDefaultAsync(m => m.Id == id);
        return new OkObjectResult(monitor);
    }

    [HttpPatch, Route("{id}")]
    public async Task<IActionResult> Update(int id, CreateMonitorModel cmm)
    {
        var validationResult = cmm.Validate();
        if (!string.IsNullOrEmpty(validationResult))
        {
            return new BadRequestObjectResult(validationResult);
        }

        try
        {
            await UpdateMonitor(id, cmm);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured updating monitor: {e}");
            return new ObjectResult("Error occured while updating monitor.") { StatusCode = 500 };
        }
        return new OkResult();
    }

    private async Task UpdateMonitor(int id, CreateMonitorModel cmm)
    {
        var monitor = _bMonitorContext.Monitors.SingleOrDefault(m => m.Id == id);
        if (monitor == null)
        {
            // todo: return notfound
            return;
        }
        monitor.Name = cmm.Name;
        monitor.UpdateIntervalInMs = Convert.ToInt32(cmm.UpdateIntervalInMs);

        if (cmm.MonitorType == "mt_ping")
        {
            UpdatePingMonitor(id, cmm, (PingMonitor)monitor);
        }
        else if (cmm.MonitorType == "mt_folder")
        {
            UpdateFolderMonitor(id, cmm, (FolderMonitor)monitor);
        }
        else if (cmm.MonitorType == "mt_http")
        {
            UpdateHttpMonitor(id, cmm, (HttpMonitor)monitor);
        }
        else if (cmm.MonitorType == "mt_sql")
        {
            UpdateSqlMonitor(id, cmm, (SqlMonitor)monitor);
        }

        await _bMonitorContext.SaveChangesAsync();
    }

    private void UpdatePingMonitor(int id, CreateMonitorModel cmm, PingMonitor monitor)
    {
        monitor.Endpoint = cmm.PingEndpoint;
        monitor.TimeoutInMs = Convert.ToInt32(cmm.PingTimeout);
    }

    private void UpdateFolderMonitor(int id, CreateMonitorModel cmm, FolderMonitor monitor)
    {
        monitor.Path = cmm.FolderPath;
    }

    private void UpdateHttpMonitor(int id, CreateMonitorModel cmm, HttpMonitor monitor)
    {
        monitor.Endpoint = cmm.HttpEndpoint;
        monitor.ExpectedStatusCode = Convert.ToInt32(cmm.HttpExpectedStatusCode);
        monitor.TimeoutInMs = Convert.ToInt32(cmm.HttpTimeout);
    }

    private void UpdateSqlMonitor(int id, CreateMonitorModel cmm, SqlMonitor monitor)
    {
        monitor.ConnectionString = cmm.SqlConnStr;
        monitor.Query = cmm.SqlQuery;
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateMonitorModel cmm)
    {
        var validationResult = cmm.Validate();
        if (!string.IsNullOrEmpty(validationResult))
        {
            return new BadRequestObjectResult(validationResult);
        }

        try
        {
            var newObject = cmm.ToDbObject();
            if (newObject == null)
            {
                _logger.LogError($"Could not determine MonitorDbObject. Cannot save.");
                return new ObjectResult("Error occured while saving monitor.") { StatusCode = 500 };
            }
            _bMonitorContext.Monitors.Add(newObject);
            await _bMonitorContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured saving new monitor: {e}");
            return new ObjectResult("Error occured while saving monitor.") { StatusCode = 500 };
        }

        return new CreatedResult(string.Empty, null);
    }
}