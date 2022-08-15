using BMonitor.Controllers.Models;
using BMonitor.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace BMonitor.Controllers;

[ApiController]
[Route("/bmonitorapi/[controller]")]
public class InformationController
{
    private readonly ILogger<InformationController> _logger;
    private readonly BMonitorContext _mc;

    public InformationController(ILogger<InformationController> logger, BMonitorContext mc)
    {
        _logger = logger;
        _mc = mc;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return new OkObjectResult(new JsonObject
        {
            ["InstanceName"] = _mc.Settings.SingleOrDefault(s => s.Key == "InstanceName")?.Value ?? "---n/a---"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateBMonitorModel cbmm)
    {
        if (string.IsNullOrWhiteSpace(cbmm.Name))
        {
            return new BadRequestObjectResult("Name needs a non-whitespace value");
        }

        try
        {
            var existingSetting = _mc.Settings.SingleOrDefault(s => s.Key == "InstanceName");
            if (existingSetting != null)
            {
                existingSetting.Value = cbmm.Name;
                await _mc.SaveChangesAsync();
                return new OkResult();
            }

            _mc.Settings.Add(new DAL.Models.Setting
            {
                Key = "InstanceName",
                Value = cbmm.Name
            });

            await _mc.SaveChangesAsync();
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured saving new BMonitor name: {e}");
            return new ObjectResult("Error occured while BMonitor name.") { StatusCode = 500 };
        }
    }
}