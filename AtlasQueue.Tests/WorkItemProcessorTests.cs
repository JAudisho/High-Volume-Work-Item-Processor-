using AtlasQueue.Domain.Entities;
using AtlasQueue.Domain.Enums;
using AtlasQueue.Infrastructure.Persistence;
using AtlasQueue.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AtlasQueue.Tests;

public class WorkItemProcessorTests
{
    private sealed class FixedClock : AtlasQueue.Domain.Abstractions.IClock
    {
        private readonly DateTime _t;
        public FixedClock(DateTime t) => _t = t;
        public DateTime UtcNow => _t;
    }

    private AppDbContext NewDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task Processes_Valid_Items_To_Completed()
    {
        using var db = NewDb(nameof(Processes_Valid_Items_To_Completed));
        var now = DateTime.UtcNow;
        var items = Enumerable.Range(1, 5).Select(i => new WorkItem
        {
            ExternalRef = $"EXT-{i}",
            PayloadJson = $$"""{"customerId":{{i}},"amount":10}""",
            Status = WorkItemStatus.Queued,
            CreatedUtc = now.AddMinutes(-i)
        }).ToList();

        await db.WorkItems.AddRangeAsync(items);
        await db.SaveChangesAsync();

        var proc = new WorkItemProcessor(db, new FixedClock(now));
        await proc.ProcessBatchAsync(items, CancellationToken.None);

        var updated = db.WorkItems.ToList();
        Assert.All(updated, it => Assert.Equal(WorkItemStatus.Completed, it.Status));
        Assert.All(updated, it => Assert.NotNull(it.ProcessedUtc));
        Assert.All(updated, it => Assert.Contains(db.WorkItemEvents.Where(e => e.WorkItemId == it.Id), e => e.Type == "COMPLETED"));
    }

    [Fact]
    public async Task Invalid_Amount_Fails_WorkItem()
    {
        using var db = NewDb(nameof(Invalid_Amount_Fails_WorkItem));
        var now = DateTime.UtcNow;
        var wi = new WorkItem
        {
            ExternalRef = "EXT-BAD",
            PayloadJson = """{"customerId":1,"amount":0}""",
            Status = WorkItemStatus.Queued,
            CreatedUtc = now.AddMinutes(-1)
        };
        await db.WorkItems.AddAsync(wi);
        await db.SaveChangesAsync();

        var proc = new WorkItemProcessor(db, new FixedClock(now));
        await proc.ProcessBatchAsync(new[] { wi }, CancellationToken.None);

        var reloaded = await db.WorkItems.FirstAsync();
        Assert.Equal(WorkItemStatus.Failed, reloaded.Status);
        Assert.False(string.IsNullOrWhiteSpace(reloaded.FailureReason));
        Assert.Contains(db.WorkItemEvents.Where(e => e.WorkItemId == reloaded.Id), e => e.Type == "FAILED");
    }
}
