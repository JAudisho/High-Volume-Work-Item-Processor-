using AtlasQueue.Domain.Entities;

namespace AtlasQueue.Domain.Abstractions;

public interface IProcessor
{
    Task ProcessBatchAsync(IEnumerable<WorkItem> items, CancellationToken ct);
}