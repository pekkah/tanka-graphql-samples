using Tanka.GraphQL;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.channels.host.logic
{
    public class Resolvers :ObjectTypeMap
    {
        public Resolvers(ResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolversMap()
            {
                {"channels", resolver.Channels},
                {"channel", resolver.Channel}
            };

            // domain
            this["Channel"] = new FieldResolversMap
            {
                {"id", Resolve.PropertyOf<Channel>(c => c.Id)},
                {"name", Resolve.PropertyOf<Channel>(c => c.Name)}
            };
        }
    }
}