using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql.samples.Host.Logic.Domain
{
    public class Chat
    {
        private readonly List<Channel> _channels = new List<Channel>();
        private readonly List<Message> _messages = new List<Message>();

        private int _nextMessageId = 1;
        private readonly BroadcastBlock<Message> _messageAdded;

        public Chat()
        {
            _messageAdded = new BroadcastBlock<Message>(message => message);
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

            await _messageAdded.SendAsync(message);
            return message;
        }

        private int NextId()
        {
            return _nextMessageId++;
        }

        public Task<IEnumerable<Message>> GetMessagesAsync(int channelId, int latest = 100)
        {
            return Task.FromResult(_messages.AsEnumerable());
        }

        public Task<Channel> CreateChannelAsync(InputChannel inputChannel)
        {
            var channel = new Channel
            {
                Id = 1,
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

        public async Task<IDisposable> JoinAsync(int channelId, BufferBlock<Message> target)
        {
            //todo: filter by channel
            var sub = _messageAdded.LinkTo(target, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            return sub;
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