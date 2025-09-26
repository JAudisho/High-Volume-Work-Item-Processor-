using AtlasQueue.Domain.Abstractions;
using AtlasQueue.Domain.Entities;
using AtlasQueue.Domain.Enums;
using AtlasQueue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasQueue.Api.Processing;

public class QueueWorker : BackgroundService
{
    private readonly ILogger<QueueWorker> _logger;
    private readonly IWorkItemChannel _channel;
    private readonly IServiceProvider _sp;
    private readonly int _batchSize;
    private readonly int _intervalMs;

    public QueueWorker(ILogger<QueueWorker> logger, IWorkItemChannel channel, IServiceProvider sp, IConfiguration cfg)
    {
        _logger = logger; _channel = channel; _sp = sp;
        _batchSize = cfg.GetValue<int>("Processing:BatchSize", 50);
        _intervalMs = cfg.GetValue<int>("Processing:DequeueIntervalMs", 250);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("QueueWorker started. BatchSize={Batch} Interval={Interval}ms", _batchSize, _intervalMs);

        var buffer = new List<WorkItem>(_batchSize);

        await foreach (var wi in _channel.ReadAllAsync(ct))
        {
            buffer.Add(wi);

            if (buffer.Count >= _batchSize)
            {
                await ProcessBuffer(buffer, ct);
                buffer.Clear();
            }

            if (_intervalMs > 0)
                await Task.Delay(_intervalMs, ct);
        }

        if (buffer.Count > 0)
            await ProcessBuffer(buffer, ct);
    }

    private async Task ProcessBuffer(List<WorkItem> buffer, CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var processor = scope.ServiceProvider.GetRequiredService<IProcessor>();

        // Re-load tracked entities to ensure state is consistent
        var ids = buffer.Select(b => b.Id).ToList();
        var tracked = await db.WorkItems.Where(w => ids.Contains(w.Id) && w.Status == WorkItemStatus.Queued)
                                        .Include(w => w.Events)
                                        .ToListAsync(ct);
        await processor.ProcessBatchAsync(tracked, ct);
        _logger.LogInformation("Processed batch of {Count}", tracked.Count);
    }
}