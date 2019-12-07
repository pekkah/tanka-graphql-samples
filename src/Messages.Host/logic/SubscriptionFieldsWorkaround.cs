using Tanka.GraphQL;
using Tanka.GraphQL.Server;

namespace tanka.graphql.samples.messages.host.logic
{
    /// <summary>
    ///     TGQL Generator currently does not handle subscriptions correctly.
    ///
    ///     Here we create new field resolvers mapping with correct dispatch
    ///     to controller.
    /// </summary>
    public class SubscriptionFieldsWorkaround : FieldResolversMap
    {
        public SubscriptionFieldsWorkaround()
        {
            Add("messageAdded",
                (context, unsubscribe) => context.Use<SubscriptionController>().MessageAdded(context, unsubscribe),
                context => context.Use<SubscriptionController>().Message(context));
        }
    }
}