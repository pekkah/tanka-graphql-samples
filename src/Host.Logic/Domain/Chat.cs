using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace fugu.graphql.samples.Host.Logic.Domain
{
    public class Chat
    {
        private List<Message> _messages = new List<Message>();

        public Task PostMessageAsync(int channelId, InputMessage message)
        {
            _messages.Add(new Message()
            {
                Content = message.Content
            });
            return Task.CompletedTask;
        }

        public IEnumerable<Message> GetMessagesAsync(int channelId, int latest = 100)
        {
            return _messages.ToArray();
        }
    }

    public class InputMessage
    {
        public string Content { get; set; }
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