using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Samples.Chat.Api.Schema;

[InterfaceType]
public partial interface IChannelEvent
{
    public int ChannelId { get; }
}

[ObjectType]
public partial class MessageChannelEvent : IChannelEvent
{
    public required int ChannelId { get; set; }
    
    public required Message Message { get; set; }
}
