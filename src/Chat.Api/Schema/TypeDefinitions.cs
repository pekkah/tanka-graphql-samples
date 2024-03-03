using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Samples.Chat.Api.Schema;

[ObjectType]
public partial class Channel
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
public partial class Message 
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
public partial class Sender
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