using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

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

        public async ValueTask<IResolveResult> ChannelMessages(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];

            var messages = await _channels.GetMessagesAsync(channelId, 100);
            return Resolve.As(messages);
        }

        public ValueTask<IResolveResult> ChannelMembers(ResolverContext context)
        {
            var member = new Member
            {
                Id = 1,
                Name = "Fugu"
            };

            return new ValueTask<IResolveResult>(Resolve.As(new[] {member}));
        }

        public async ValueTask<IResolveResult> PostMessage(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];
            var inputMessage = context.GetArgument<InputMessage>("message");

            var message = await _channels.PostMessageAsync(channelId, inputMessage); //todo: fix bug in lib

            return Resolve.As(message);
        }

        public async ValueTask<IResolveResult> Channel(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];
            var channel = await _channels.GetChannelAsync(channelId);

            return Resolve.As(channel);
        }

        public ValueTask<ISubscribeResult> SubscribeToChannel(ResolverContext context, CancellationToken unsubscribe)
        {
            var channelId = (int) (long) context.Arguments["channelId"];
            return new ValueTask<ISubscribeResult>(_channels.Join(channelId, unsubscribe));
        }

        public ValueTask<IResolveResult> Message(ResolverContext context)
        {
            return new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue));
        }
    }
}