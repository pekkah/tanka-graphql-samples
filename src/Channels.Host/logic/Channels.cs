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
            _channels.AddRange(new[]
            {
                new Channel
                {
                    Id = 1,
                    Name = "General"
                },
                new Channel
                {
                    Id = 2,
                    Name = "Misc"
                }
            });
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

    public partial class Channel
    {
        public Channel()
        {
            Name = string.Empty;
        }
    }
}