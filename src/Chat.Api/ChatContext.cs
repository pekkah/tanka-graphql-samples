using Microsoft.EntityFrameworkCore;

namespace Tanka.GraphQL.Samples.Chat.Api;

public class ChatContext : DbContext
{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {
    }

    public DbSet<Channel> Channels => Set<Channel>();

    public DbSet<Message> Messages => Set<Message>();
}