using System;
using System.IO;
using System.Reflection;
using System.Text;
using tanka.graphql.sdl;
using tanka.graphql.type;

namespace tanka.graphql.samples.Chat.Host.Schemas
{
    public static class SchemaLoader
    {
        public static SchemaBuilder Load()
        {
            var idl = LoadIdlFromResource();

            var builder = new SchemaBuilder();
            Sdl.Import(Parser.ParseDocument(idl), builder);

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
                assembly.GetManifestResourceStream("tanka.graphql.samples.Chat.Host.Schemas.Chat.graphql");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}