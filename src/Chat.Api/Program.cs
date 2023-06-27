using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Samples.Chat;
using Tanka.GraphQL.Samples.Chat.Api;
using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddPooledDbContextFactory<ChatContext>(
        options => options.UseSqlite(
            "Data Source=Chat.db;Cache=Shared"
        )
    );

builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        options.AddGeneratedTypes(types =>
        {
            types.AddTankaGraphQLSamplesChatTypes();
        });
    });

builder.Services
    .AddAuthentication("github")
    .AddScheme<GitHubAccessTokenAuthenticationOptions, GitHubAccessTokenAuthenticationHandler>("github", options =>
    {
    });

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    IDbContextFactory<ChatContext> dbFactory = app.Services.GetRequiredService<IDbContextFactory<ChatContext>>();
    await using ChatContext db = await dbFactory.CreateDbContextAsync();
    await db.Database.EnsureDeletedAsync();
    try
    {
        await db.Database.EnsureCreatedAsync();

        await db.Channels.AddRangeAsync(new Channel { Name = "General", Description = "" },
            new Channel { Name = "Tanka", Description = "" });
        await db.SaveChangesAsync();
    }
    catch (Exception x)
    {
        app.Services.GetRequiredService<ILogger<Program>>().LogError(x, "Failed to initialize database");
        throw;
    }
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.Run();