using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Tanka.GraphQL.Samples.Chat.Api;

public interface IChannelEvents
{
    ValueTask Publish<T>(T channelEvent, CancellationToken cancellationToken) where T : ChannelEvent;

    IAsyncEnumerable<ChannelEvent> Subscribe(int channelId, CancellationToken cancellationToken);
}

public class ChannelEvents : IChannelEvents
{
    private readonly ConcurrentDictionary<int, ChannelBroadcaster> _broadcasters = new();

    public async ValueTask Publish<T>(T channelEvent, CancellationToken cancellationToken) where T : ChannelEvent
    {
        ChannelBroadcaster channelBroadcaster = _broadcasters
            .GetOrAdd(channelEvent.ChannelId, _ => new ChannelBroadcaster());

        await channelBroadcaster.Publish(channelEvent, cancellationToken);
    }

    public async IAsyncEnumerable<ChannelEvent> Subscribe(int channelId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ChannelBroadcaster channelBroadcaster = _broadcasters
            .GetOrAdd(channelId, _ => new ChannelBroadcaster());

        await foreach(var ev in channelBroadcaster.Subscribe(cancellationToken))
            yield return ev;
    }
}

public abstract record ChannelEvent(int ChannelId, string EventType);

public record MessageChannelEvent(
    int ChannelId,
    string EventType,
    Message Message) : ChannelEvent(ChannelId, EventType);

public class ChannelBroadcaster
{
    private readonly BroadcasterChannel<ChannelEvent> _broadcaster = new();


    public async Task Publish<T>(T ev, CancellationToken cancellationToken) where T : ChannelEvent
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _broadcaster.WriteAsync(ev);
    }

    public IAsyncEnumerable<ChannelEvent> Subscribe(CancellationToken cancellationToken)
    {
        var output = System.Threading.Channels.Channel.CreateUnbounded<ChannelEvent>();
        var unsubscribe = _broadcaster.Subscribe(output.Writer);

        cancellationToken.Register(() =>
        {
            unsubscribe.Dispose();
        });

        return output.Reader.ReadAllAsync(CancellationToken.None);
    }
}