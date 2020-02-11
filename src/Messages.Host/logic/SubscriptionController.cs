using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class SubscriptionController : SubscriptionControllerBase<Subscription>
    {
        private readonly Messages _channels;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SubscriptionController(Messages channels, IHttpContextAccessor httpContextAccessor)
        {
            _channels = channels;
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<ISubscriberResult> MessageAdded(Subscription? objectValue, int channelId, CancellationToken unsubscribe,
            IResolverContext context)
        {
            return new ValueTask<ISubscriberResult>(_channels.Join(channelId, unsubscribe));
        }

        public override ValueTask<Message> MessageAdded(object objectValue, int channelId, IResolverContext context)
        {
            var message = (Message)context.ObjectValue;
            return new ValueTask<Message>(message);
        }
    }
}