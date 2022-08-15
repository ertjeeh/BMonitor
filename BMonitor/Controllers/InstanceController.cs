using BMonitor.Controllers.Models;
using BMonitor.DAL;
using BMonitor.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMonitor.Controllers;

[ApiController]
[Route("/bmonitorapi/[controller]")]
public class InstanceController
{
    private readonly ILogger<InstanceController> _logger;
    private readonly BMonitorContext _bMonitorContext;

    public InstanceController(ILogger<InstanceController> logger, BMonitorContext bMonitorContext)
    {
        _logger = logger;
        _bMonitorContext = bMonitorContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var instances = await _bMonitorContext.Instances
            .Include(i => i.Cards)
            .ToListAsync();
        return new OkObjectResult(instances);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateInstanceModel cim)
    {
        if (string.IsNullOrWhiteSpace(cim.Name))
        {
            return new BadRequestObjectResult("Name needs a non-whitespace value");
        }

        try
        {
            if (_bMonitorContext.Instances.Any(i => i.Name == cim.Name))
            {
                return new BadRequestObjectResult(
                    $"Another instance with name: \"{cim.Name}\" already exists. Please choose a different name.");
            }

            _bMonitorContext.Instances.Add(new Instance
            {
                Name = cim.Name,
                Description = cim.Description
            });
            await _bMonitorContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured saving new instance: {e}");
            return new ObjectResult("Error occured while saving instance.") { StatusCode = 500 };
        }

        return new CreatedResult(string.Empty, null);
    }

    [HttpDelete, Route("Card")]
    public async Task<IActionResult> DeleteCardFromInstance(DeleteCardFromInstanceModel dcfim)
    {
        if (dcfim.InstanceId <= 0 || dcfim.CardId <= 0)
        {
            return new BadRequestObjectResult("No InstanceId/CardId provided.");
        }

        try
        {
            var instance = await _bMonitorContext.Instances.Include(i => i.Cards).SingleOrDefaultAsync(i => i.Id == dcfim.InstanceId);
            if (instance == null)
            {
                return new BadRequestObjectResult("Could not find instance.");
            }

            var card = instance.Cards.SingleOrDefault(c => c.Id == dcfim.CardId);
            if (card == null)
            {
                return new BadRequestObjectResult("Could not find card.");
            }

            instance.Cards.Remove(card);
            await _bMonitorContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured deleting card {dcfim.CardId} from instance {dcfim.InstanceId}: {e}");
            return new ObjectResult("Error occured deleting card from instance.") { StatusCode = 500 };
        }

        return new OkResult();
    }
}