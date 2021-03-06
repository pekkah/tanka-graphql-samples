﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Tanka.GraphQL.Introspection;
using tanka.graphql.samples.channels.host.logic;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.Server.Links;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace tanka.graphql.samples.channels.host
{
    public class SchemaCache
    {
        private readonly IMemoryCache _cache;

        public SchemaCache(
            IMemoryCache cache)
        {
            _cache = cache;
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
                var schemaBuilder = await SchemaLoader.Load();
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