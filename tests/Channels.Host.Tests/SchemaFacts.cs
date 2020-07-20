using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Tanka.GraphQL;
using Tanka.GraphQL.Introspection;
using tanka.graphql.samples.channels.host;
using Xunit;

namespace Channels.Host.Tests
{
    public class SchemaFacts
    {
        public SchemaFacts()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _sut = new SchemaCache(_cache);
        }

        private readonly MemoryCache _cache;
        private readonly SchemaCache _sut;

        [Fact]
        public async Task Add()
        {
            /* Given */
            /* When */
            var schema = await _sut.GetOrAdd();

            /* Then */
            Assert.NotNull(schema);
            Assert.NotNull(schema.Query);
        }

        [Fact]
        public async Task Query_Introspection()
        {
            /* Given */
            var query = Introspect.DefaultQuery;

            var schema = await _sut.GetOrAdd();
            var introspectionSchema = Introspect.Schema(schema);

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = introspectionSchema,
                Document = query
            });

           /* Then */
           Assert.Null(result.Errors);
        }
    }
}