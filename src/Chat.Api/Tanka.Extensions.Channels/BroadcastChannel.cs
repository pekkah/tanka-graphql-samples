using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Tanka.Extensions.Channels;

public class BroadcastChannel<T>
{
    private readonly ConcurrentDictionary<Channel<T>, byte> _channels = new();

    public IAsyncEnumerable<T> Subscribe(CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false
        });
        _channels.TryAdd(channel, 0);

        cancellationToken.Register(Remove);

        return new AsyncEnumerable(channel.Reader, Remove);

        void Remove()
        {
            _channels.TryRemove(channel, out _);
            channel.Writer.TryComplete();
        }
    }

    public int SubscriberCount => _channels.Count;

    public async ValueTask Publish(T item, CancellationToken cancellationToken = default)
    {
        foreach (var (channel, _) in _channels)
        {
            await channel.Writer.WriteAsync(item, cancellationToken);
        }
    }

    private class AsyncEnumerable(ChannelReader<T> reader, Action onDisposed)
        : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator(reader, onDisposed, cancellationToken);
        }
    }

    private class AsyncEnumerator : IAsyncEnumerator<T>
    {
        private bool _disposed;
        private readonly ChannelReader<T> _reader;
        private readonly Action _onDisposed;

        public AsyncEnumerator(ChannelReader<T> reader, Action onDisposed, CancellationToken cancellationToken)
        {
            _reader = reader;
            _onDisposed = onDisposed;
            cancellationToken.Register(onDisposed);
        }

        public T Current { get; private set; } = default!;

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _onDisposed();
            _disposed = true;
            await _reader.Completion;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            try
            {
                Current = await _reader.ReadAsync();
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
