using AtlasQueue.Api.Processing;
using AtlasQueue.Domain.DTO;
using AtlasQueue.Domain.Entities;
using AtlasQueue.Domain.Enums;
using AtlasQueue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AtlasQueue.Api.Controllers;

[ApiController]
[Route("api/work-items")]
public class WorkItemsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWorkItemChannel _channel;
    private readonly IMemoryCache _cache;

    public WorkItemsController(AppDbContext db, IWorkItemChannel channel, IMemoryCache cache)
    {
        _db = db;
        _channel = channel;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<WorkItemDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] WorkItemStatus? status = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var key = $"list:{page}:{pageSize}:{q}:{status}";
        if (_cache.TryGetValue(key, out var cachedObj) && cachedObj is PagedResult<WorkItemDto> cached)
            return Ok(cached);

        IQueryable<WorkItem> query = _db.WorkItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(w => w.ExternalRef.Contains(q));

        if (status.HasValue)
            query = query.Where(w => w.Status == status);

        var total = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(w => w.CreatedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkItemDto(
                w.Id,
                w.ExternalRef,
                w.Status,
                w.Priority,
                w.CreatedUtc,
                w.ProcessedUtc,
                w.FailureReason))
            .ToListAsync();

        var result = new PagedResult<WorkItemDto>(items, page, pageSize, total);
        _cache.Set(key, result, TimeSpan.FromSeconds(10));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WorkItemDto>> Create([FromBody] CreateWorkItemDto dto, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var wi = new WorkItem
        {
            ExternalRef = dto.ExternalRef,
            PayloadJson = dto.PayloadJson,
            Priority = dto.Priority,
            CreatedUtc = now
        };

        await _db.WorkItems.AddAsync(wi, ct);
        wi.Events.Add(new WorkItemEvent { WorkItemId = wi.Id, Type = "QUEUED", CreatedUtc = now });
        await _db.SaveChangesAsync(ct);

        await _channel.QueueAsync(wi, ct);

        var result = new WorkItemDto(
            wi.Id, wi.ExternalRef, wi.Status, wi.Priority, wi.CreatedUtc, wi.ProcessedUtc, wi.FailureReason);

        return CreatedAtAction(nameof(GetById), new { id = wi.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkItemDto>> GetById(Guid id)
    {
        var w = await _db.WorkItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (w is null) return NotFound();

        return Ok(new WorkItemDto(
            w.Id, w.ExternalRef, w.Status, w.Priority, w.CreatedUtc, w.ProcessedUtc, w.FailureReason));
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id)
    {
        var wi = await _db.WorkItems.Include(x => x.Events).FirstOrDefaultAsync(x => x.Id == id);
        if (wi is null) return NotFound();

        wi.Status = WorkItemStatus.Queued;
        wi.FailureReason = null;
        wi.Events.Add(new WorkItemEvent { WorkItemId = wi.Id, Type = "RETRIED", CreatedUtc = DateTime.UtcNow });

        await _db.SaveChangesAsync();
        await _channel.QueueAsync(wi, HttpContext.RequestAborted);

        return NoContent();
    }
}