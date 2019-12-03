using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class ResolverService
    {
        private readonly Messages _channels;

        public ResolverService(Messages channels)
        {
            _channels = channels;
        }

        public async ValueTask<IResolverResult> ChannelMessages(IResolverContext context)
        {
            var channelId = (int) context.Arguments["channelId"];

            var messages = await _channels.GetMessagesAsync(channelId);
            return Resolve.As(messages);
        }

        public async ValueTask<IResolverResult> PostMessage(IResolverContext context)
        {
            var channelId = (int) context.Arguments["channelId"];
            var inputMessage = context.GetObjectArgument<InputMessage>("message");

            // current user is being injected by the resolver middleware
            var user = (ClaimsPrincipal) context.Items["user"];

            // use name claim from the profile if present otherwise use default name claim (sub)
            var from = user.FindFirstValue("name") ?? user.Identity.Name;

            // use profile picture claim from the profile if present otherwise leave empty
            var picture = user.FindFirstValue("picture") ?? string.Empty;

            var message = await _channels.PostMessageAsync(channelId, from, picture, inputMessage);
            return Resolve.As(message);
        }

        public ValueTask<ISubscriberResult> SubscribeToChannel(IResolverContext context, CancellationToken unsubscribe)
        {
            var channelId = (int) context.Arguments["channelId"];
            return new ValueTask<ISubscriberResult>(_channels.Join(channelId, unsubscribe));
        }

        public ValueTask<IResolverResult> Message(IResolverContext context)
        {
            return new ValueTask<IResolverResult>(Resolve.As(context.ObjectValue));
        }
    }
}