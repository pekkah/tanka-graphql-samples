using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Samples.Chat.Api.Schema;

[InputType]
public partial class ChannelCommand
{
    public AddMessageCommand? AddMessage { get; set; }

    public AddChannelCommand? AddChannel { get; set; }
}

[InputType]
public partial class AddMessageCommand
{
    public int ChannelId { get; set; }

    public string Content { get; set; } = string.Empty;
}

[InputType]
public partial class AddChannelCommand
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public partial class CommandResult
{
}

[ObjectType]
public partial class AddChannelResult(Channel channel) : CommandResult
{
    public Channel Channel { get; } = channel;
}

[ObjectType]
public partial class AddMessageResult(Message message) : CommandResult
{
    public Message Message { get; } = message;
}
