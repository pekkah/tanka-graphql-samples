using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.samples.Host.Logic.Domain;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.Host.Logic.Schemas
{
    public class ChatResolvers : ResolverMap
    {
        public ChatResolvers(ChatResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolverMap
            {
                {"channels", resolver.Channels},
                {"channel", resolver.Channel},
                {"members", resolver.ChannelMembers},
                {"messages", resolver.ChannelMessages}
            };

            this["Mutation"] = new FieldResolverMap()
            {
                {"postMessage", resolver.PostMessage}
            };

            this["Subscription"] = new FieldResolverMap();

            // domain
            this["Channel"] = new FieldResolverMap
            {
                {"id", PropertyOf<Channel>(c => c.Id)},
                {"name", PropertyOf<Channel>(c => c.Name)}
            };

            this["Member"] = new FieldResolverMap
            {
                {"id", PropertyOf<Member>(c => c.Id)},
                {"name", PropertyOf<Member>(c => c.Name)}
            };

            this["Message"] = new FieldResolverMap
            {
                {"id", PropertyOf<Message>(m => m.Id)},
                {"content", PropertyOf<Message>(m => m.Content)}
            };
        }
    }

    public class ChatResolverService
    {
        private readonly Chat _chat;

        public ChatResolverService(Chat chat)
        {
            _chat = chat;
        }

        public async Task<IResolveResult> Channels(ResolverContext context)
        {
            var channels = await _chat.GetChannelsAsync();
            return As(channels);
        }

        public async Task<IResolveResult> ChannelMessages(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];

            var messages = await _chat.GetMessagesAsync(channelId, 100);
            return As(messages);
        }

        public Task<IResolveResult> ChannelMembers(ResolverContext context)
        {
            var member = new Member
            {
                Id = 1,
                Name = "Fugu"
            };

            return Task.FromResult(As(new[] {member}));
        }

        public async Task<IResolveResult> PostMessage(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];
            var inputMessage = context.GetArgument<InputMessage>("message");

            var message = await _chat.PostMessageAsync(channelId, inputMessage); //todo: fix bug in lib

            return As(message);
        }

        public async Task<IResolveResult> Channel(ResolverContext context)
        {
            var channelId = (int) (long) context.Arguments["channelId"];
            var channel = await _chat.GetChannelAsync(channelId);

            return As(channel);
        }
    }
}