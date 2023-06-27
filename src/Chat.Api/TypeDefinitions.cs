using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat;

[ObjectType]
public static class Query
{
    public static async Task<IEnumerable<Channel>> Channels(
        [FromServices] IDbContextFactory<ChatContext> dbFactory,
        ResolverContext context
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
        string description
    )
    {
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

    public required string Name { get; init; }

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
        string text,
        ResolverContext context
    )
    {
        //todo: need to simplify both getting the HttpContext and the User
        var user = context.QueryContext.Features.GetRequiredFeature<IHttpContextFeature>().HttpContext.User;
        await using var db = await dbFactory.CreateDbContextAsync();
        var message = new Message
        {
            Text = text,
            ChannelId = _channel.Id,
            TimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            Sender = new Sender()
            {
                Name = user.FindFirstValue(JwtRegisteredClaimNames.Name) ?? "??",
                Sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "-1",
                AvatarUrl = user.FindFirstValue("avatar_url") ?? string.Empty
            }
        };
        await db.Messages.AddAsync(message);
        await db.SaveChangesAsync();
        return message;
    }
}

[ObjectType]
public class Message 
{
    public int Id { get; set; }

    public required string TimestampMs { get; set; }

    public required string Text { get; init; }

    public Sender Sender { get; set; } = null!;

    public required int ChannelId { get; init; }
}

[ObjectType]
[Owned]
public class Sender
{
    public string Sub { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

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