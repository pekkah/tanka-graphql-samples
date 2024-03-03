using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Api.Schema;

[ObjectType]
public static partial class Query
{
    public static async Task<IEnumerable<Channel>> Channels(
        [FromServices] IDbContextFactory<ChatContext> dbFactory
    )
    {
        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        return await db.Channels.ToListAsync();
    }

    public static async Task<Channel?> Channel(
        [FromArguments] int id,
        [FromServices] IDbContextFactory<ChatContext> dbFactory
    )
    {
        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        var channel = await db.Channels.FindAsync(id);

        return channel;
    }
}

[ObjectType]
public static partial class Mutation
{
    public static async Task<CommandResult> Execute(
        ResolverContext context,
        [FromServices] IDbContextFactory<ChatContext> dbFactory,
        [FromServices] IChannelEvents events,
        [FromArguments] ChannelCommand command
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

            await events.Publish(new MessageChannelEvent()
            {
                ChannelId = channelId,
                Message = message
            }, CancellationToken.None);
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
public partial class Subscription
{
    public static IAsyncEnumerable<IChannelEvent> ChannelEvents(
        SubscriberContext context,
        [FromArguments]int id,
        CancellationToken cancellationToken
    )
    {
        var events = context.GetRequiredService<IChannelEvents>();
        return events.Subscribe(id, cancellationToken);
    }
}