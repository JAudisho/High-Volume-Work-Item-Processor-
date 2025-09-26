using AtlasQueue.Domain.Abstractions;

namespace AtlasQueue.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}