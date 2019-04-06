using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.channels;
using tanka.graphql.resolvers;

namespace tanka.graphql.samples.channels.host.logic
{
    public class Channels
    {
        private readonly List<Channel> _channels = new List<Channel>();
        private readonly PoliteEventChannel<Message> _messageAdded;
        private readonly List<Message> _messages = new List<Message>();

        private int _nextMessageId = 1;

        public Channels()
        {
            _messageAdded = new PoliteEventChannel<Message>(new Message
            {
                Id = -1,
                Content = "Welcome!"
            });

            _channels.Add(new Channel
            {
                Id = 1,
                Name = "General"
            });
        }

        public async Task<Message> PostMessageAsync(int channelId, InputMessage inputMessage)
        {
            var channel = await GetChannelAsync(channelId);

            if (channel == null) throw new InvalidOperationException($"Channel '{channelId}' not found");

            var message = new Message
            {
                Id = NextId(),
                Content = inputMessage.Content
            };
            _messages.Add(message);

            await _messageAdded.WriteAsync(message);
            return message;
        }

        public Task<IEnumerable<Message>> GetMessagesAsync(int channelId, int latest = 100)
        {
            return Task.FromResult(_messages.AsEnumerable());
        }

        public Task<Channel> CreateChannelAsync(InputChannel inputChannel)
        {
            var channel = new Channel
            {
                Id = _channels.Count + 1,
                Name = inputChannel.Name
            };

            _channels.Add(channel);

            return Task.FromResult(channel);
        }

        public Task<Channel> GetChannelAsync(int channelId)
        {
            var channel = _channels.SingleOrDefault(c => c.Id == channelId);
            return Task.FromResult(channel);
        }

        public Task<IEnumerable<Channel>> GetChannelsAsync()
        {
            return Task.FromResult(_channels.AsEnumerable());
        }

        public ISubscribeResult Join(int channelId, CancellationToken unsubscribe)
        {
            return _messageAdded.Subscribe(unsubscribe);
        }

        private int NextId()
        {
            return _nextMessageId++;
        }
    }

    public class InputMessage
    {
        public string Content { get; set; }
    }

    public class InputChannel
    {
        public string Name { get; set; }
    }

    public class Channel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Member
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }


    public class Message
    {
        public int Id { get; set; }

        public string Content { get; set; }
    }
}