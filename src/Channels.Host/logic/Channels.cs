using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tanka.graphql.samples.channels.host.logic
{
    public class Channels
    {
        private readonly List<Channel> _channels = new List<Channel>();

        public Channels()
        {
            _channels.Add(new Channel
            {
                Id = 1,
                Name = "General"
            });
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
}