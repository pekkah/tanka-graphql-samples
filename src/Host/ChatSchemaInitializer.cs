using System.Threading.Tasks;
using fugu.graphql.samples.Host.AsyncInitializer;
using fugu.graphql.samples.Host.Logic.Domain;
using fugu.graphql.samples.Host.Logic.Schemas;
using fugu.graphql.type;
using static fugu.graphql.tools.SchemaTools;

namespace fugu.graphql.samples.Host
{
    public class ChatSchemaInitializer : IAsyncInitializer
    {
        public ISchema Schema { get; set; }

        public async Task InitializeAsync()
        {
            var idl = await SchemaLoader.LoadAsync();
            await idl.InitializeAsync();

            var chat = new Chat();
            var channel = await chat.CreateChannelAsync(new InputChannel()
            {
                Name = "General"
            });
            await chat.PostMessageAsync(channel.Id, new InputMessage()
            {
                Content = "Hardcoded message from initializer"
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