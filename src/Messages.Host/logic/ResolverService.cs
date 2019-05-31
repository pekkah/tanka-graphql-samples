using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.samples.messages.host.logic
{
    public class ResolverService
    {
        private readonly Messages _channels;

        public ResolverService(Messages channels)
        {
            _channels = channels;
        }

        public async ValueTask<IResolveResult> ChannelMessages(ResolverContext context)
        {
            var channelId = (int) context.Arguments["channelId"];

            var messages = await _channels.GetMessagesAsync(channelId);
            return Resolve.As(messages);
        }

        public async ValueTask<IResolveResult> PostMessage(ResolverContext context)
        {
            var channelId = (int) context.Arguments["channelId"];
            var inputMessage = context.GetArgument<InputMessage>("message");

            // current user is being injected by the resolver middleware
            var user = (ClaimsPrincipal)context.Items["user"];

            // use email claim if present otherwise use default name claim (sub)
            var from = user.FindFirstValue("email") ?? user.Identity.Name;
            if (from.Contains("@"))
                from = from.Substring(0, from.IndexOf("@", StringComparison.Ordinal));

            var message = await _channels.PostMessageAsync(channelId, from, inputMessage);
            return Resolve.As(message);
        }

        public ValueTask<ISubscribeResult> SubscribeToChannel(ResolverContext context, CancellationToken unsubscribe)
        {
            var channelId = (int) context.Arguments["channelId"];
            return new ValueTask<ISubscribeResult>(_channels.Join(channelId, unsubscribe));
        }

        public ValueTask<IResolveResult> Message(ResolverContext context)
        {
            return new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue));
        }
    }
}