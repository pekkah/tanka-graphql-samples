using System.Threading.Tasks;
using fugu.graphql.resolvers;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.Host.Schemas
{
    public class ChatResolvers : ResolverMap
    {
        public ChatResolvers(ChatResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolverMap
            {
                {"hello", resolver.Hello}
            };

            this["Mutation"] = new FieldResolverMap();
            this["Subscription"] = new FieldResolverMap();

            // domain
            this["Hello"] = new FieldResolverMap()
            {
                {"message", PropertyOf<Message>(m => m.Content)}
            };
        }
    }

    public class ChatResolverService
    {
        public Task<IResolveResult> Hello(ResolverContext context)
        {
            var message = new Message
            {
                Content = "world"
            };

            return Task.FromResult(As(message));
        }
    }


    public class Message
    {
        public string Content { get; set; }
    }
}