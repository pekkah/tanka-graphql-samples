using System.Threading.Tasks;
using tanka.graphql.samples.Host.AsyncInitializer;
using tanka.graphql.samples.Host.Logic.Domain;
using tanka.graphql.samples.Host.Logic.Schemas;
using tanka.graphql.type;
using static tanka.graphql.tools.SchemaTools;

namespace tanka.graphql.samples.Host
{
    public class ChatSchemaInitializer : IAsyncInitializer
    {
        public ISchema Schema { get; set; }

        public async Task InitializeAsync()
        {
            var idl = await SchemaLoader.LoadAsync();

            var chat = new Chat();
            await chat.CreateChannelAsync(new InputChannel()
            {
                Name = "General"
            });

            var service = new ChatResolverService(chat);
            var resolvers = new ChatResolvers(service);

            Schema = await MakeExecutableSchemaWithIntrospection(
                idl,
                resolvers,
                resolvers);
        }
    }
}