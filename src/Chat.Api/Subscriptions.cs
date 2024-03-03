using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using Tanka.GraphQL.Samples.Chat.Api.Schema;

namespace Tanka.GraphQL.Samples.Chat.Api;

public interface IChannelEvents
{
    ValueTask Publish<T>(T channelEvent, CancellationToken cancellationToken) where T : IChannelEvent;

    IAsyncEnumerable<IChannelEvent> Subscribe(int channelId, CancellationToken cancellationToken);
}

public class ChannelEvents : IChannelEvents
{
    private readonly ConcurrentDictionary<int, ChannelBroadcaster> _broadcasters = new();

    public async ValueTask Publish<T>(T channelEvent, CancellationToken cancellationToken) where T : IChannelEvent
    {
        ChannelBroadcaster channelBroadcaster = _broadcasters
            .GetOrAdd(channelEvent.ChannelId, _ => new ChannelBroadcaster());

        await channelBroadcaster.Publish(channelEvent, cancellationToken);
    }

    public async IAsyncEnumerable<IChannelEvent> Subscribe(int channelId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ChannelBroadcaster channelBroadcaster = _broadcasters
            .GetOrAdd(channelId, _ => new ChannelBroadcaster());

        await foreach(var ev in channelBroadcaster.Subscribe(cancellationToken))
            yield return ev;
    }
}

public class ChannelBroadcaster
{
    private readonly EventAggregator<IChannelEvent> _broadcaster = new();


    public async Task Publish<T>(T ev, CancellationToken cancellationToken) where T : IChannelEvent
    {
        await _broadcaster.Publish(ev, cancellationToken);
    }

    public IAsyncEnumerable<IChannelEvent> Subscribe(CancellationToken cancellationToken)
    {
        return _broadcaster.Subscribe(cancellationToken);
    } 
}