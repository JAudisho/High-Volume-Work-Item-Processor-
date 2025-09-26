using AtlasQueue.Domain.Enums;
using AtlasQueue.Domain.ValueObjects;

namespace AtlasQueue.Domain.Entities;

public class WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalRef { get; set; } = default!; // e.g., upstream system id
    public string PayloadJson { get; set; } = default!; // store dynamic data
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Queued;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime CreatedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
    public string? FailureReason { get; set; }

    public List<WorkItemEvent> Events { get; set; } = new();
}
