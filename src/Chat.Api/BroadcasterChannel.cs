using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

namespace Tanka.GraphQL.Samples.Chat.Api;

public class CancellationSource : IDisposable
{
    private readonly CancellationTokenSource _source;

    public CancellationSource(CancellationTokenSource source)
    {
        _source = source;
    }

    public void Dispose()
    {
        if (_source.IsCancellationRequested)
            _source.Dispose();
        else
        {
            _source.Cancel();
            _source.Dispose();
        }
    }
}

public class BroadcasterChannel<T>: IAsyncDisposable
{
    private readonly ConcurrentDictionary<CancellationTokenSource, ChannelWriter<T>> _subscribers = new();

    public IDisposable Subscribe(ChannelWriter<T> observer, bool completeOnUnsubscribe = true)
    {
        var cts = new CancellationTokenSource();

        if (!_subscribers.TryAdd(cts, observer))
        {
            throw new InvalidOperationException("Should not happen");
        }

        cts.Token.Register(() =>
        {
            if(_subscribers.TryRemove(cts, out var subscriber))
            {
                if (completeOnUnsubscribe)
                    subscriber.TryComplete();
            }
        });

        return new CancellationSource(cts);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var subscriber in _subscribers)
        {
            if (!subscriber.Key.IsCancellationRequested)
            {
                await subscriber.Key.CancelAsync();
                subscriber.Key.Dispose();
            }
        }
    }

    public async ValueTask WriteAsync(T item)
    {
        if (_subscribers.IsEmpty)
            return; // should probably throw but ..example

        await Parallel.ForEachAsync(_subscribers, CancellationToken.None, WriteItem);

        ValueTask WriteItem(KeyValuePair<CancellationTokenSource, ChannelWriter<T>> subscriber, CancellationToken _)
        {
            var unsubscribe = subscriber.Key.Token;
            var writer = subscriber.Value;

            if (unsubscribe.IsCancellationRequested)
            {
                return ValueTask.CompletedTask;
            }

            return writer.TryWrite(item) ? ValueTask.CompletedTask : Core(writer, item, unsubscribe);

            static async ValueTask Core(ChannelWriter<T> writer, T item, CancellationToken cancellationToken)
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested && await writer.WaitToWriteAsync(cancellationToken))
                    {
                        if (writer.TryWrite(item))
                            return;
                    }
                }
                catch (TaskCanceledException ex) when(ex.CancellationToken == cancellationToken)
                {
                    // unsubscribe should not throw 
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}