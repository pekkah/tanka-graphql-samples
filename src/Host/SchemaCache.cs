using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Server.Links;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace tanka.graphql.samples.Host
{
    public class SchemaCache
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public SchemaCache(
            IConfiguration configuration,
            IMemoryCache cache,
            IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _cache = cache;
            _accessor = accessor;
        }

        public Task<ISchema> GetOrAdd()
        {
            return _cache.GetOrCreateAsync(
                "Schema",
                Create);
        }

        private async Task<ISchema> Create(ICacheEntry cacheEntry)
        {
            cacheEntry.SetSlidingExpiration(TimeSpan.FromHours(6));

            try
            {
                // create channelsSchema by introspecting channels service
                var channelsLink = Links.SignalROrHttp(
                    _configuration["Remotes:Channels"],
                    _configuration["Remotes:ChannelsHttp"],
                    _accessor);

                var channelsSchema = RemoteSchemaTools.MakeRemoteExecutable(
                    await new SchemaBuilder()
                        .ImportIntrospectedSchema(channelsLink),
                    channelsLink);

                // create messagesSchema by introspecting messages service
                var messagesLink = Links.SignalR(
                    _configuration["Remotes:Messages"],
                    _accessor);

                var messagesSchema = RemoteSchemaTools.MakeRemoteExecutable(
                    await new SchemaBuilder()
                        .ImportIntrospectedSchema(messagesLink),
                    messagesLink);

                // combine schemas into one
                var schema = new SchemaBuilder()
                    .Merge(channelsSchema, messagesSchema)
                    .Build();

                // introspect and merge with schema
                var introspection = Introspect.Schema(schema);
                return new SchemaBuilder()
                    .Merge(schema, introspection)
                    .Build();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}