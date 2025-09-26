using System.Text.Json;
using AtlasQueue.Domain.Abstractions;
using AtlasQueue.Domain.Entities;
using AtlasQueue.Domain.Enums;
using AtlasQueue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AtlasQueue.Infrastructure.Services;

public class WorkItemProcessor : IProcessor
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public WorkItemProcessor(AppDbContext db, IClock clock)
    {
        _db = db; _clock = clock;
    }

    public async Task ProcessBatchAsync(IEnumerable<WorkItem> items, CancellationToken ct)
    {
        // Simple retry with jitter for transient DB issues
        var retry = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(100 * i + Random.Shared.Next(50)));

        await retry.ExecuteAsync(async () =>
        {
            foreach (var wi in items)
            {
                wi.Status = WorkItemStatus.Processing;
                wi.Events.Add(new WorkItemEvent { WorkItemId = wi.Id, Type = "STARTED", CreatedUtc = _clock.UtcNow });
            }

            await _db.SaveChangesAsync(ct);

            foreach (var wi in items)
            {
                try
                {
                    // Pretend to do CPU/IO-heavy work (parse & validate)
                    var doc = JsonDocument.Parse(wi.PayloadJson);
                    var amount = doc.RootElement.TryGetProperty("amount", out var a) ? a.GetInt32() : 0;
                    if (amount <= 0) throw new InvalidOperationException("Amount must be positive.");

                    // “Business result”
                    wi.Status = WorkItemStatus.Completed;
                    wi.ProcessedUtc = _clock.UtcNow;
                    wi.Events.Add(new WorkItemEvent { WorkItemId = wi.Id, Type = "COMPLETED", Data = $"amount={amount}", CreatedUtc = _clock.UtcNow });
                }
                catch (Exception ex)
                {
                    wi.Status = WorkItemStatus.Failed;
                    wi.FailureReason = ex.Message;
                    wi.Events.Add(new WorkItemEvent { WorkItemId = wi.Id, Type = "FAILED", Data = ex.Message, CreatedUtc = _clock.UtcNow });
                }
            }

            await _db.SaveChangesAsync(ct);
        });
    }
}