using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class SubscriptionController
    {
        private readonly Messages _channels;

        public SubscriptionController(Messages channels)
        {
            _channels = channels;
        }

        public ValueTask<ISubscriberResult> MessageAdded(IResolverContext context, CancellationToken unsubscribe)
        {
            var channelId = (int) context.Arguments["channelId"];
            return new ValueTask<ISubscriberResult>(_channels.Join(channelId, unsubscribe));
        }

        public ValueTask<IResolverResult> Message(IResolverContext context)
        {
            return new ValueTask<IResolverResult>(Resolve.As(context.ObjectValue));
        }
    }
}