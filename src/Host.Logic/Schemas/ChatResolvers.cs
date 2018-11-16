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
                {"channel", resolver.Channel}
            };

            this["Mutation"] = new FieldResolverMap();
            this["Subscription"] = new FieldResolverMap();

            // domain
            this["Channel"] = new FieldResolverMap
            {
                {"id", PropertyOf<Channel>(c => c.Id)},
                {"name", PropertyOf<Channel>(c => c.Name)},
                {"members", resolver.ChannelMembers},
                {"messages", resolver.ChannelMessages}
            };

            this["Member"] = new FieldResolverMap
            {
                {"id", PropertyOf<Member>(c => c.Id)},
                {"name", PropertyOf<Member>(c => c.Name)}
            };

            this["Message"] = new FieldResolverMap
            {
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

        public async Task<IResolveResult> Channel(ResolverContext context)
        {
            var id = (string)context.Arguments["id"]; //todo: fix bug in lib side
            var channel = await _chat.GetChannelAsync(int.Parse(id));
            return As(channel);
        }

        public async Task<IResolveResult> ChannelMessages(ResolverContext context)
        {
            var channel = (Channel) context.ObjectValue;
            var latest = (int)(long) context.Arguments["latest"]; //todo: ix bug in lib side

            var messages = await _chat.GetMessagesAsync(channel.Id, latest);
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
    }
}