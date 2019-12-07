using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.channels.host.logic
{
    public class QueryController : QueryControllerBase<Query>
    {
        private readonly Channels _channels;

        public QueryController(Channels channels)
        {
            _channels = channels;
        }

        public override async ValueTask<IEnumerable<Channel>> Channels(Query? objectValue, IResolverContext context)
        {
            var channels = await _channels.GetChannelsAsync();
            return channels;
        }

        public override async ValueTask<Channel?> Channel(Query? objectValue, int channelId, IResolverContext context)
        {
            var channel = await _channels.GetChannelAsync(channelId);
            return channel;
        }
    }
}