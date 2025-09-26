namespace AtlasQueue.Domain.Entities;

public class WorkItemEvent
{
    public long Id { get; set; }
    public Guid WorkItemId { get; set; }
    public string Type { get; set; } = default!; // QUEUED, STARTED, COMPLETED, FAILED, RETRIED
    public string? Data { get; set; }
    public DateTime CreatedUtc { get; set; }
}