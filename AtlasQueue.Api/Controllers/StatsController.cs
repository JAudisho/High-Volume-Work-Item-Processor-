using AtlasQueue.Domain.Enums;
using AtlasQueue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtlasQueue.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<object>> Get()
    {
        var now = DateTime.UtcNow;
        var lastHour = now.AddHours(-1);

        var total = await _db.WorkItems.LongCountAsync();

        var byStatus = await _db.WorkItems
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.LongCount() })
            .ToListAsync();

        var completedLastHour = await _db.WorkItems
            .Where(w => w.Status == WorkItemStatus.Completed && w.ProcessedUtc >= lastHour)
            .LongCountAsync();

        double? avgLatencyMs = await _db.WorkItems
            .Where(w => w.ProcessedUtc != null)
            .Select(w => EF.Functions.DateDiffMillisecond(w.CreatedUtc, w.ProcessedUtc!.Value))
            .AverageAsync();

        var failed = byStatus.FirstOrDefault(s => s.Status == WorkItemStatus.Failed)?.Count ?? 0;
        var completed = byStatus.FirstOrDefault(s => s.Status == WorkItemStatus.Completed)?.Count ?? 0;

        return Ok(new
        {
            total,
            byStatus = byStatus.ToDictionary(k => k.Status.ToString(), v => v.Count),
            completedLastHour,
            averageLatencyMs = avgLatencyMs,
            successRate = total == 0 ? 0 : Math.Round(100.0 * completed / total, 2),
            failureRate = total == 0 ? 0 : Math.Round(100.0 * failed / total, 2),
            serverUtc = now
        });
    }
}