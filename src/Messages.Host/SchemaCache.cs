using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using tanka.graphql.samples.messages.host.logic;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace tanka.graphql.samples.messages.host
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
                // load typeDefs
                var schemaBuilder = await SchemaLoader.Load();

                // bind the actual field value resolvers and create schema
                var resolvers = new SchemaResolvers();
                var schema = SchemaTools.MakeExecutableSchemaWithIntrospection(
                    schemaBuilder,
                    resolvers,
                    resolvers);

                return schema;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}