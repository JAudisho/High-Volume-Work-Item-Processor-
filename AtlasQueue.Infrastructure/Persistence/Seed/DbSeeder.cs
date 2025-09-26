using AtlasQueue.Domain.Entities;
using AtlasQueue.Domain.Enums;
using AtlasQueue.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtlasQueue.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.WorkItems.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var items = Enumerable.Range(1, 50).Select(i => new WorkItem
            {
                ExternalRef = $"EXT-{1000 + i}",
                PayloadJson = $$"""{"customerId":{{i}},"amount":{{(i*3)%17+10}}}""",
                Priority = i % 7 == 0 ? Priority.High : Priority.Medium,
                Status = WorkItemStatus.Queued,
                CreatedUtc = now.AddMinutes(-i)
            }).ToList();

            await db.WorkItems.AddRangeAsync(items);
            await db.SaveChangesAsync();
        }
    }
}