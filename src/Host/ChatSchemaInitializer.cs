using System.Threading.Tasks;
using fugu.graphql.samples.Host.AsyncInitializer;
using fugu.graphql.samples.Host.Schemas;
using fugu.graphql.tools;
using fugu.graphql.type;

namespace fugu.graphql.samples.Host
{
    public class ChatSchemaInitializer : IAsyncInitializer
    {
        public ISchema Schema { get; set; }

        public async Task InitializeAsync()
        {
            var idl = await SchemaLoader.LoadAsync();
            await idl.InitializeAsync();

            var service = new ChatResolverService();
            var resolvers = new ChatResolvers(service);

            Schema = await SchemaTools.MakeExecutableSchemaWithIntrospection(
                idl,
                resolvers,
                resolvers);
        }
    }
}