using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Samples.Chat.Api;
using Tanka.GraphQL.Samples.Chat.Api.Startup;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Vite.AspNetCore;
using Vite.AspNetCore.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddPooledDbContextFactory<ChatContext>(
        options => options.UseSqlite(
            "Data Source=Chat.db;Cache=Shared"
        )
    );

builder.Services.AddSingleton<IChannelEvents, ChannelEvents>();

// add Tanka GraphQL and configure schema
builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        // Add source generator generated types for queries and mutations
        options.AddGeneratedTypes(types =>
        {
            types.AddTankaGraphQLSamplesChatApiTypes();
        });

        // Manually configure subscription types as the SG does not support it yet
        //TODO: Add subscription support to the code generator
        options.Configure(configure =>
        {
            ExecutableSchemaBuilder schema = configure.Builder;

            schema.Add("""
                       interface ChannelEvent {
                           channelId: Int!
                           eventType: String!
                       }

                       type MessageChannelEvent implements ChannelEvent
                       """);

            schema.Add(new ObjectResolversConfiguration(
                "MessageChannelEvent",
                new FieldsWithResolvers
                {
                    { "channelId: Int!", (MessageChannelEvent objectValue) => objectValue.ChannelId },
                    { "eventType: String!", (MessageChannelEvent objectValue) => objectValue.EventType },
                    { "message: Message!", (MessageChannelEvent objectValue) => objectValue.Message }
                }));

            schema.Add("Subscription",
                new FieldsWithResolvers
                {
                    {
                        "channel_events(id: Int!): ChannelEvent", (ResolverContext context) =>
                        {
                            context.ResolvedValue = context.ObjectValue;
                            context.ResolveAbstractType = (definition, value) => value switch
                            {
                                MessageChannelEvent => context.Schema.GetRequiredNamedType<ObjectDefinition>(
                                    "MessageChannelEvent"),
                                _ => throw new InvalidOperationException("Unknown ChannelEvent type")
                            };
                        }
                    }
                },
                new FieldsWithSubscribers
                {
                    {
                        "channel_events(id: Int!): ChannelEvent", b => b.Run((context, unsubscribe) =>
                        {
                            var events = context.GetRequiredService<IChannelEvents>();
                            int id = context.GetArgument<int>("id");
                            context.ResolvedValue = events.Subscribe(id, unsubscribe);
                            return default;
                        })
                    }
                });
        });
    });

// Add GitHub authentication with cookies
builder.AddCookieAndGitHubAuthentication();

// These are used for the UI
builder.Services.AddRazorPages(); // host the SolidJS UI
builder.Services.AddViteServices(new ViteOptions() // use vite development server
{
    PackageDirectory = "UI",
    Server = new ViteServerOptions
    {
        AutoRun = false, // enable to autostart vite dev server
        Https = true,
        UseFullDevUrl = true
    }
});

// Write schema files to disk for graphql-codegen
if (builder.Environment.IsDevelopment()) 
    builder.Services.AddSingleton<IHostedService, WriteSchemaFiles>();

/* App */
WebApplication app = builder.Build();

// Run migrations in development
if (app.Environment.IsDevelopment()) await app.RunMigrations();

app.UseSecurityHeaders();
app.UseHttpsRedirection();

// This is used during production to serve the SolidJS UI resources
if (!app.Environment.IsDevelopment()) app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// WebSockets are required to use websockets transport with GraphQL
app.UseWebSockets();

// Some utility endpoints for the SolidJS UI
app.UseBffEndpoints();

// Map Default schema to endpoint
app.MapTankaGraphQL("/graphql", "Default");

// SolidJS UI is hosted by Razor page
app.MapRazorPages();
if (app.Environment.IsDevelopment())
    // Server SolidJS resources in development
    app.UseViteDevMiddleware();

// Allow SolidJS router to handle unknown requests
app.MapFallbackToPage("/Index");
app.Run();