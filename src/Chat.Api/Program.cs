using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Samples.Chat;
using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
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

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var dbFactory = app.Services.GetRequiredService<IDbContextFactory<ChatContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.Run();

