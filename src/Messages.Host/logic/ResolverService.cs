﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using tanka.graphql.resolvers;

namespace tanka.graphql.samples.messages.host.logic
{
    public class ResolverService
    {
        private readonly Messages _channels;
        private readonly IHttpContextAccessor _contextAccessor;

        public ResolverService(Messages channels, IHttpContextAccessor contextAccessor)
        {
            _channels = channels;
            _contextAccessor = contextAccessor;
        }

        public async ValueTask<IResolveResult> ChannelMessages(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];

            var messages = await _channels.GetMessagesAsync(channelId, 100);
            return Resolve.As(messages);
        }

        public async ValueTask<IResolveResult> PostMessage(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];
            var inputMessage = context.GetArgument<InputMessage>("message");

            inputMessage.Content = $"{_contextAccessor.HttpContext.User.Identity.Name} - {inputMessage.Content}";
            var message = await _channels.PostMessageAsync(channelId, inputMessage);

            return Resolve.As(message);
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