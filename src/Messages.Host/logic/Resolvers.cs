using tanka.graphql.resolvers;

namespace tanka.graphql.samples.messages.host.logic
{
    public class Resolvers : ResolverMap
    {
        public Resolvers(ResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolverMap
            {
                {"messages", resolver.ChannelMessages}
            };

            this["Mutation"] = new FieldResolverMap()
            {
                {"postMessage", resolver.PostMessage}
            };

            this["Subscription"] = new FieldResolverMap()
            {
                {"messageAdded", resolver.SubscribeToChannel, resolver.Message}
            };

            this["Message"] = new FieldResolverMap
            {
                {"id", Resolve.PropertyOf<Message>(m => m.Id)},
                {"channelId", Resolve.PropertyOf<Message>(m => m.Id)},
                {"content", Resolve.PropertyOf<Message>(m => m.Content)},
                {"timestamp", Resolve.PropertyOf<Message>(m => m.Timestamp?.ToString("O"))},
                {"from", Resolve.PropertyOf<Message>(m => m.From)},
            };
        }
    }
}