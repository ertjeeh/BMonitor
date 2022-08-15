using BMonitor.Controllers.Models;
using BMonitor.DAL;
using BMonitor.DAL.Models;
using BMonitor.DAL.Models.Cards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BMonitor.Controllers;

[ApiController]
[Route("/bmonitorapi/[controller]")]
public class CardController
{
    private readonly ILogger<CardController> _logger;
    private readonly BMonitorContext _bMonitorContext;

    public CardController(ILogger<CardController> logger, BMonitorContext bMonitorContext)
    {
        _logger = logger;
        this._bMonitorContext = bMonitorContext;
    }

    [HttpGet, Route("Html")]
    public async Task<IActionResult> GetById([FromQuery] int cardId, [FromQuery] bool applyTransformations = true)
    {
        var card = _bMonitorContext.HtmlCards.FirstOrDefault(h => h.Id == cardId);
        if (card == null)
        {
            return new NotFoundResult();
        }

        if (applyTransformations)
        {
            await ApplyTransformations(card);
        }

        return new OkObjectResult(card);
    }

    [HttpPost, Route("Html")]
    public async Task<IActionResult> Post(CreateHtmlCardModel chcm)
    {
        if (string.IsNullOrWhiteSpace(chcm.Title))
        {
            return new BadRequestObjectResult("No title provided for new card.");
        }

        if (chcm.InstanceId <= 0)
        {
            return new BadRequestObjectResult("Incorrect instance selected to create card to.");
        }

        if (string.IsNullOrWhiteSpace(chcm.Html))
        {
            return new BadRequestObjectResult("No HTML provided for new card.");
        }

        try
        {
            var instance = await _bMonitorContext.Instances
                .Include(i => i.Cards)
                .SingleOrDefaultAsync(i => i.Id == chcm.InstanceId);
            if (instance == null)
            {
                return new BadRequestObjectResult($"Could not find instance to add the new card to.");
            }

            var newCard = _bMonitorContext.HtmlCards.Add(new HtmlCard
            {
                Title = chcm.Title,
                Html = chcm.Html,
            });

            instance.Cards.Add(newCard.Entity);
            await _bMonitorContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured saving new card: {e}");
            return new ObjectResult("Error occured while saving card.") { StatusCode = 500 };
        }

        return new CreatedResult(string.Empty, null);
    }

    [HttpPatch, Route("Html/{id}")]
    public async Task<IActionResult> Update(int id, CreateHtmlCardModel chcm)
    {
        if (string.IsNullOrWhiteSpace(chcm.Title))
        {
            return new BadRequestObjectResult("No title provided for card.");
        }

        if (string.IsNullOrWhiteSpace(chcm.Html))
        {
            return new BadRequestObjectResult("No HTML provided for new card.");
        }

        try
        {
            var card = _bMonitorContext.HtmlCards.SingleOrDefault(h => h.Id == id);
            if (card == null)
            {
                return new NotFoundResult();
            }

            card.Title = chcm.Title;
            card.Html = chcm.Html;
            await _bMonitorContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured updating card: {e}");
            return new ObjectResult("Error occured while updating card.") { StatusCode = 500 };
        }

        return new CreatedResult(string.Empty, null);
    }

    private async Task ApplyTransformations(HtmlCard card)
    {
        var refreshTimes = new List<DateTime>();

        // find like ^IconOnline:Green^
        foreach (Match m in new Regex(@"\^Icon([^:]+)(:[^\^]+){0,1}\^").Matches(card.Html))
        {
            var spanColor = m.Groups.Count > 2
                ? $" style=\"color: {m?.Groups[2].Value.Replace(":", string.Empty).ToLower()}\""
                : string.Empty;

            var icon = GetIcon(m.Groups[1].Value);
            var replaceWith = $@"<span {icon} {spanColor}>&nbsp;</span>";
            card.Html = card.Html.Replace(m.Groups[0].Value, replaceWith);
        }

        // find like ^Graph|1:IntResult|24|Avg|5^
        foreach (Match m in new Regex(@"\^Graph\|(\d+):([^|]+)\|(\d+)\|(Avg|Max|Min)\|(\d+)\^").Matches(card.Html))
        {
            try
            {
                var monitorId = Convert.ToInt64(m.Groups[1].Value);
                var prop = m.Groups[2].Value;
                var hoursBack = Convert.ToInt32(m.Groups[3].Value);
                var what = m.Groups[4].Value;
                var groupByMinutes = Convert.ToInt32(m.Groups[5].Value);
                var interval = TimeSpan.FromMinutes(groupByMinutes).Ticks;
                // var result = _monContext.MonitorResults
                //     .Include(m => m.Monitor)
                //     .Where(m => m.Id == monitorId)
                //     .OrderByDescending(m => m.DateTime)
                //     .Where(m => m.DateTime > DateTime.Now.AddHours(-hoursBack))
                //     //.GroupBy(m => m.DateTime.Ticks / interval )
                //     .ToList();

                var result = await _bMonitorContext.MonitorResults
                        .Include(mr => mr.Monitor)
                        // .OrderByDescending(mr => mr.DateTime)
                        .Where(mr => mr.MonitorId == monitorId)
                        .Where(m => m.DateTime > DateTime.Now.AddHours(-hoursBack))

                        //.Select(g => new { dt = new DateTime(g.Key.Date.Ticks).AddHours(g.Key.Hour), v = g.Average(x => x.IntResult) })
                        //.Select(s => new {series = s, dt = s.First().DateTime, avg = s.Average(x => x.IntResult)})
                        .ToListAsync();

                var r = result
                    //.GroupBy(m => m.DateTime.Ticks / interval)
                    //.Select(g => new { dt = new DateTime(g.Key), v = g.Average(x => x.IntResult) })
                    .GroupBy(x => x.DateTime.Ticks / TimeSpan.FromMinutes(groupByMinutes).Ticks)
                    .Select(s => new
                    {
                        dt = s.First().DateTime,
                        avg = s.Average(x => x.IntResult)
                    })
                    .ToList();

                if (what.ToLower() == "avg" && prop.ToLower() == "intresult")
                {
                    //                    card.Html = card.Html.Replace(m.Groups[0].Value, GenerateChart(r.Select(y => (y.dt, y.v))));
                    card.Html = card.Html.Replace(m.Groups[0].Value, GenerateChart(r.Select(y => (y.dt, y.avg))));
                }
            }
            catch (Exception)
            {
                // todo make sure error is visible to gui
            }
        }

        // find like ^1:IntResult^
        foreach (Match m in new Regex(@"\^(\d*):(\S*)\^").Matches(card.Html))
        {
            try
            {
                var monitorId = m.Groups[1].Value;
                var prop = m.Groups[2].Value;
                var result = await _bMonitorContext.MonitorResults
                    .OrderByDescending(m => m.DateTime)
                    .Include(m => m.Monitor)
                    .FirstAsync(m => m.MonitorId == Convert.ToInt32(monitorId));
                var value = GetValue(prop, result);
                card.Html = card.Html.Replace(m.Groups[0].Value, value);

                var nextRefresh = result.DateTime.AddMilliseconds(result.Monitor.UpdateIntervalInMs);
                refreshTimes.Add(nextRefresh);
            }
            catch (Exception)
            {
                // todo make sure error is visible to gui
                continue;
            }
        }

        // find like ^If|1:IntResult<51|V^
        foreach (Match m in new Regex(@"\^If\|(\d+):([^<|>]+)(<|>)([^\|]+)\|([^\^]+)\^").Matches(card.Html))
        {
            try
            {
                var completeMatch = m.Groups[0].Value;
                var monitorId = m.Groups[1].Value;
                var prop = m.Groups[2].Value;
                var op = m.Groups[3].Value;
                var target = m.Groups[4].Value;
                var replaceWith = m.Groups[5].Value;

                var result = await _bMonitorContext.MonitorResults
                    .OrderByDescending(m => m.DateTime)
                    .Include(m => m.Monitor)
                    .FirstAsync(m => m.MonitorId == Convert.ToInt32(monitorId));
                var value = GetValue(prop, result);
                card.Html = card.Html.Replace(completeMatch, IfIsTrue(op, value, target) ? replaceWith : string.Empty);

                var nextRefresh = result.DateTime.AddMilliseconds(result.Monitor.UpdateIntervalInMs);
                refreshTimes.Add(nextRefresh);
            }
            catch (Exception)
            {
                // todo make sure error is visible to gui
                continue;
            }
        }

        if (!refreshTimes.Any())
        {
            return;
        }

        var earliestRefresh = refreshTimes.Where(r => refreshTimes.Min().AddSeconds(3) > r).Max();
        card.Html += $"<!-- ^EarliestRefresh:{earliestRefresh:o}Z^ -->";
    }

    private static string? GenerateChart(IEnumerable<(DateTime d, double? avg)> input)
    {
        var graphName = $"graphconfig_{new Random().Next(1, 4096)}";
        var x = input.OrderBy(i => i.d).Select(i => @$"{{""x"":""{ToJsDateTime(i.d)}"",""y"":""{(Math.Round(i.avg.Value, 0))}""}}");
        var d = $"[{string.Join(",", x)}]";
        return @$"
<div style=""height: 75%""><canvas id=""c_{graphName}""></canvas></div>
<!--GenGraph:{graphName}^{d}^-->
";
    }

    private static string ToJsDateTime(DateTime t) => t.ToString("o") + "Z";

    private static string GetIcon(string value)
    {
        return value.ToLower() switch
        {
            "online" => @" class=""neu-so-checkmark""",
            "offline" => @" class=""neu-so-information-triangle""",
            _ => string.Empty
        };
    }

    private static bool IfIsTrue(string op, string value, string target)
    {
        return op switch
        {
            "<" => Convert.ToInt32(value) < Convert.ToInt32(target),
            ">" => Convert.ToInt32(value) > Convert.ToInt32(target),
            _ => false
        };
    }

    private static string GetValue(string prop, MonitorResult result)
    {
        if (prop == "IntResult")
        {
            return result.IntResult.ToString();
        }

        throw new NotImplementedException();
    }
}