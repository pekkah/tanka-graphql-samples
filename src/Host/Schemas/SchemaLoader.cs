using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using fugu.graphql.sdl;
using fugu.graphql.type;

namespace fugu.graphql.samples.Host.Schemas
{
    public static class SchemaLoader
    {
        public static async Task<ISchema> LoadAsync()
        {
            var idl = await LoadIdlFromResourcesAsync();
            var schema = Sdl.Schema(Parser.ParseDocument(idl));

            return schema;
        }

        /// <summary>
        ///     Load schema from embedded resource
        /// </summary>
        /// <returns></returns>
        private static async Task<string> LoadIdlFromResourcesAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream =
                assembly.GetManifestResourceStream("fugu.graphql.samples.Host.Schemas.Chat.gql");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}