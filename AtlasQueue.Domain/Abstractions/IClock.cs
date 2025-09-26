namespace AtlasQueue.Domain.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}