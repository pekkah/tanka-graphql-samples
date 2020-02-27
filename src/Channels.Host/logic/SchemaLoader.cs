using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;

namespace tanka.graphql.samples.channels.host.logic
{
    public static class SchemaLoader
    {
        public static async Task<SchemaBuilder> Load()
        {
            var idl = LoadIdlFromResource();

            var builder = await new SchemaBuilder()
                .SdlAsync(idl);

            return builder;
        }

        /// <summary>
        ///     Load schema from embedded resource
        /// </summary>
        /// <returns></returns>
        private static string LoadIdlFromResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream =
                assembly.GetManifestResourceStream("tanka.graphql.samples.channels.host.logic.Schema.graphql");

            using var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}