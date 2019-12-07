using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class QueryController : QueryControllerBase<Query>
    {
        private readonly Messages _messages;

        public QueryController(Messages messages)
        {
            _messages = messages;
        }

        public override async ValueTask<IEnumerable<Message>> Messages(Query? objectValue, int channelId, IResolverContext context)
        {
            var messages = await _messages.GetMessagesAsync(channelId);
            return messages;
        }
    }
}