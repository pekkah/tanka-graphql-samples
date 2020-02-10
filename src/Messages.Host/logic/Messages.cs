using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class Messages
    {
        private readonly ConcurrentDictionary<int, EventChannel<Message>> _channels;
        private readonly List<Message> _messages = new List<Message>();

        private int _nextMessageId = 1;

        public Messages()
        {
            _channels = new ConcurrentDictionary<int, EventChannel<Message>>();
        }

        public async Task<Message> PostMessageAsync(int channelId, string from, string profileUrl,
            InputMessage inputMessage)
        {
            var message = new Message
            {
                Id = NextId(),
                Content = inputMessage.Content,
                ChannelId = channelId,
                From = from,
                ProfileUrl = profileUrl
            };
            _messages.Add(message);

            await PostMessage(message);
            return message;
        }

        private async Task PostMessage(Message message)
        {
            if (_channels.TryGetValue(message.ChannelId, out var channel)) await channel.WriteAsync(message);
        }

        public Task<IEnumerable<Message>> GetMessagesAsync(int channelId, int latest = 100)
        {
            return Task.FromResult(_messages
                .Where(m => m.ChannelId == channelId)
                .OrderBy(m => m.Timestamp)
                .AsEnumerable());
        }

        public ISubscriberResult Join(int channelId, CancellationToken unsubscribe)
        {
            var channel = _channels.GetOrAdd(channelId, _ => new EventChannel<Message>());
            var result = channel.Subscribe(unsubscribe);
            return result;
        }

        private int NextId()
        {
            return _nextMessageId++;
        }
    }
}