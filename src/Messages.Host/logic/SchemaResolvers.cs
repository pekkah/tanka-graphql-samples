namespace tanka.graphql.samples.messages.host.logic
{
    public partial class SchemaResolvers
    {
        partial void Modify()
        {
            Remove("Subscription");
            Add("Subscription", new SubscriptionFieldsWorkaround());
        }
    }
}