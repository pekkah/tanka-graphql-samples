﻿using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.channels.host.logic
{
    public class ResolverService
    {
        private readonly Channels _channels;

        public ResolverService(Channels channels)
        {
            _channels = channels;
        }

        public async ValueTask<IResolveResult> Channels(ResolverContext context)
        {
            var channels = await _channels.GetChannelsAsync();
            return Resolve.As(channels);
        }

        public async ValueTask<IResolveResult> Channel(ResolverContext context)
        {
            var channelId = (int) context.Arguments["channelId"];
            var channel = await _channels.GetChannelAsync(channelId);

            return Resolve.As(channel);
        }
    }
}