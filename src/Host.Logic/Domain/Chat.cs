using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fugu.graphql.type;

namespace fugu.graphql.samples.Host.Logic.Domain
{
    public class Chat
    {
        private List<Message> _messages = new List<Message>();
        private List<Channel> _channels = new List<Channel>();

        public async Task PostMessageAsync(int channelId, InputMessage message)
        {
            var channel = await GetChannelAsync(channelId);

            if (channel == null)
            {
                throw new InvalidOperationException($"Channel '{channelId}' not found");
            }

            _messages.Add(new Message()
            {
                Content = message.Content
            });
        }

        private Task<Channel> GetChannelAsync(int channelId)
        {
            var channel = _channels.SingleOrDefault(c => c.Id == channelId);
            return Task.FromResult(channel);
        }

        public IEnumerable<Message> GetMessagesAsync(int channelId, int latest = 100)
        {
            return _messages.ToArray();
        }

        public Task CreateChannelAsync(InputChannel inputChannel)
        {
            var channel = new Channel()
            {
                Id = 1,
                Name = inputChannel.Name
            };

            _channels.Add(channel);

            return Task.CompletedTask;
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
        public string Content { get; set; }
    }
}