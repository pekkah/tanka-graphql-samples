using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tanka.GraphQL.Server;
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
    public static async Task<MutationChannel> AddChannel(
        [FromServices] IDbContextFactory<ChatContext> dbFactory,
        string name,
        string description,
        ResolverContext context
    )
    {
        var user = context.GetUser();

        if (user.Identity?.IsAuthenticated == false)
            throw new InvalidOperationException($"Forbidden: user is not authenticated.");

        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        var channel = new Channel
        {
            Name = name,
            Description = description
        };
        await db.Channels.AddAsync(channel);
        await db.SaveChangesAsync();
        return new MutationChannel(channel);
    }

    public static async Task<MutationChannel?> Channel(
        [FromServices] IDbContextFactory<ChatContext> dbFactory,
        int id
    )
    {
        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        var channel = await db.Channels
            .FindAsync(id);

        if (channel is null)
            return null;

        return new MutationChannel(channel);
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
public class MutationChannel
{
    private readonly Channel _channel;

    public MutationChannel(Channel channel)
    {
        _channel = channel;
    }

    public int Id => _channel.Id;

    public string Name => _channel.Name;

    public string Description => _channel.Description;

    public async Task<Message> AddMessage(
        [FromServices] IDbContextFactory<ChatContext> dbFactory,
        [FromServices] IChannelEvents events,
        string text,
        ResolverContext context
    )
    {
        var user = context.GetUser();

        if (user.Identity?.IsAuthenticated == false)
            throw new InvalidOperationException($"Forbidden: user is not authenticated.");

        await using var db = await dbFactory.CreateDbContextAsync();
        var message = new Message
        {
            Text = text,
            ChannelId = _channel.Id,
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

        await events.Publish(new MessageChannelEvent(_channel.Id, nameof(MessageChannelEvent), message), CancellationToken.None);
        return message;
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

//todo: Use SG for DbContext?
public class ChatContext : DbContext
{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {
    }

    public DbSet<Channel> Channels => Set<Channel>();

    public DbSet<Message> Messages => Set<Message>();
}