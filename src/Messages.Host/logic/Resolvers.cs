using Tanka.GraphQL;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class Resolvers : ObjectTypeMap
    {
        public Resolvers(ResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolversMap()
            {
                {"messages", resolver.ChannelMessages}
            };

            this["Mutation"] = new FieldResolversMap
            {
                {"postMessage", resolver.PostMessage}
            };

            this["Subscription"] = new FieldResolversMap
            {
                {"messageAdded", resolver.SubscribeToChannel, resolver.Message}
            };

            this["Message"] = new FieldResolversMap
            {
                {"id", Resolve.PropertyOf<Message>(m => m.Id)},
                {"channelId", Resolve.PropertyOf<Message>(m => m.Id)},
                {"content", Resolve.PropertyOf<Message>(m => m.Content)},
                {"timestamp", Resolve.PropertyOf<Message>(m => m.Timestamp?.ToString("O"))},
                {"from", Resolve.PropertyOf<Message>(m => m.From)},
                {"profileUrl", Resolve.PropertyOf<Message>(m => m.ProfileUrl)}
            };
        }
    }
}