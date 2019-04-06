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
            };

            // domain
            this["Channel"] = new FieldResolverMap
            {
                {"id", Resolve.PropertyOf<Channel>(c => c.Id)},
                {"name", Resolve.PropertyOf<Channel>(c => c.Name)}
            };
        }
    }
}