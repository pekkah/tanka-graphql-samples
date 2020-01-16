using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class SubscriptionController : SubscriptionControllerBase<Subscription>
    {
        private readonly Messages _channels;

        public SubscriptionController(Messages channels)
        {
            _channels = channels;
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