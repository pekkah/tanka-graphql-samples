using Microsoft.EntityFrameworkCore;

namespace Tanka.GraphQL.Samples.Chat.Api;

public class ChatContext : DbContext
{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {
    }

    public DbSet<Schema.Channel> Channels => Set<Schema.Channel>();

    public DbSet<Schema.Message> Messages => Set<Schema.Message>();
}