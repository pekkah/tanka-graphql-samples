using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Api;

[ObjectType]
public static class Query
{
    public static async Task<IEnumerable<Channel>> Channels(
        [FromServices] IDbContextFactory<ChatContext> dbFactory
    )
    {
        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        return await db.Channels.ToListAsync();
    }

    public static async Task<Channel?> Channel(
        [FromArguments]int id,
        [FromServices] IDbContextFactory<ChatContext> dbFactory
    )
    {
        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        var channel = await db.Channels.FindAsync(id);

        return channel;
    }
}

[ObjectType]
public static class Mutation
{
    public static async Task<CommandResult> Execute(
        ResolverContext context,
        [FromServices] IDbContextFactory<ChatContext> dbFactory,
        [FromServices] IChannelEvents events,
        [FromArguments]ChannelCommand command
        )
    {
        context.ResolveAbstractType = (definition, value) => value switch
        {
            AddChannelResult => context.Schema.GetRequiredNamedType<ObjectDefinition>("AddChannelResult"),
            AddMessageResult => context.Schema.GetRequiredNamedType<ObjectDefinition>("AddMessageResult"),
            _ => throw new InvalidOperationException($"Unknown type {definition.Name}")
        };
        
        var user = context.GetUser();

        if (user.Identity?.IsAuthenticated == false)
            throw new InvalidOperationException($"Forbidden: user is not authenticated.");

        await using var db = await dbFactory.CreateDbContextAsync();
        
        if (command.AddMessage is not null)
        {
            var text = command.AddMessage.Content;
            var channelId = command.AddMessage.ChannelId;
            
            var message = new Message
            {
                Text = text,
                ChannelId = channelId,
                TimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                Sender = new Sender()
                {
                    Name = user.FindFirstValue("name") ?? "??",
                    Id = user.FindFirstValue("id") ?? "-1",
                    Login = user.FindFirstValue("login") ?? string.Empty,
                    AvatarUrl = user.FindFirstValue("avatar_url") ?? string.Empty
                }
            };
            await db.Messages.AddAsync(message);
            await db.SaveChangesAsync();

            await events.Publish(new MessageChannelEvent(channelId, nameof(MessageChannelEvent), message), CancellationToken.None);
            return new AddMessageResult(message);
        }

        ArgumentNullException.ThrowIfNull(command.AddChannel);

        var name = command.AddChannel.Name;
        var description = command.AddChannel.Description;
        var channel = new Channel
        {
            Name = name,
            Description = description
        };
        await db.Channels.AddAsync(channel);
        await db.SaveChangesAsync();
        return new AddChannelResult(channel);
    }
}

[ObjectType]
public class Channel
{
    public int Id { get; set; }

    [MaxLength(1024)]
    public required string Name { get; init; }

    [MaxLength(2048)]
    public required string Description { get; init; }

    public async Task<IEnumerable<Message>> Messages(
        [FromServices] IDbContextFactory<ChatContext> dbFactory
        )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var messages = await db.Messages            
            .Where(message => message.ChannelId == Id)
            .ToListAsync();

        return messages;
    }
}

[ObjectType]
public class Message 
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string TimestampMs { get; set; }

    [MaxLength(4048)]
    public required string Text { get; init; }

    public Sender Sender { get; set; } = null!;

    public required int ChannelId { get; init; }
}

[ObjectType]
[Owned]
public class Sender
{
    [MaxLength(100)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string AvatarUrl { get; set; } = string.Empty;
}

public abstract class CommandResult
{

}

[ObjectType]
public class AddChannelResult : CommandResult
{
    public AddChannelResult(Channel channel)
    {
        Channel = channel;
    }

    public Channel Channel { get; }
}

[ObjectType]
public class AddMessageResult : CommandResult
{
    public AddMessageResult(Message message)
    {
        Message = message;
    }

    public Message Message { get; }
}

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