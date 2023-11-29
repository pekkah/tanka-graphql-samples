using Microsoft.EntityFrameworkCore;

namespace Tanka.GraphQL.Samples.Chat.Api.Startup;

public static class MigrationExtensions
{
    public static async Task RunMigrations(this WebApplication app)
    {
        var dbFactory =
            app.Services.GetRequiredService<IDbContextFactory<ChatContext>>();
        await using ChatContext db = await dbFactory.CreateDbContextAsync();
        await db.Database.EnsureDeletedAsync();
        try
        {
            await db.Database.EnsureCreatedAsync();

            await db.Channels.AddRangeAsync(
                new Channel { Name = "General", Description = "" },
                new Channel { Name = "Tanka", Description = "" }
            );

            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            app.Services.GetRequiredService<ILogger<Program>>().LogError(x, "Failed to initialize database");
            throw;
        }
    }
}