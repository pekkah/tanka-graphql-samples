using tanka.graphql.resolvers;

namespace tanka.graphql.samples.channels.host.logic
{
    public class Resolvers : ResolverMap
    {
        public Resolvers(ResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolverMap
            {
                {"channels", resolver.Channels},
                {"channel", resolver.Channel},
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

            // domain
            this["Channel"] = new FieldResolverMap
            {
                {"id", Resolve.PropertyOf<Channel>(c => c.Id)},
                {"name", Resolve.PropertyOf<Channel>(c => c.Name)}
            };

            this["Message"] = new FieldResolverMap
            {
                {"id", Resolve.PropertyOf<Message>(m => m.Id)},
                {"content", Resolve.PropertyOf<Message>(m => m.Content)}
            };
        }
    }
}