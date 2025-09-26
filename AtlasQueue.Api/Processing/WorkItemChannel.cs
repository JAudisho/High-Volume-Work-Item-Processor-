using AtlasQueue.Domain.Entities;
using System.Threading.Channels;

namespace AtlasQueue.Api.Processing;

public interface IWorkItemChannel
{
    ValueTask QueueAsync(WorkItem item, CancellationToken ct);
    IAsyncEnumerable<WorkItem> ReadAllAsync(CancellationToken ct);
}

public class WorkItemChannel : IWorkItemChannel
{
    private readonly Channel<WorkItem> _channel;

    public WorkItemChannel(int capacity = 10_000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<WorkItem>(options);
    }

    public ValueTask QueueAsync(WorkItem item, CancellationToken ct) => _channel.Writer.WriteAsync(item, ct);

    public IAsyncEnumerable<WorkItem> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}