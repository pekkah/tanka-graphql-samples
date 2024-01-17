using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Extensions.Experimental.OneOf;
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
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

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
        
        options.PostConfigure(configure =>
        {
            // this is experimental feature adding support for @oneOf input types
            configure.Builder.Schema.AddOneOf();
            
            //todo: add support to the code generator
            configure.Builder.Add("""
                               extend input ChannelCommand @oneOf
                               """);
        });

        options.PostConfigure(configure =>
        {
            //todo: add support to code generator
            configure.Builder.Schema.Add("""
                                         union CommandResult = AddChannelResult | AddMessageResult
                                         """);
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

builder.Services.AddOneOfValidationRule();

// Add GitHub authentication with cookies
builder.AddCookieAndGitHubAuthentication();

// These are used for the UI
builder.Services.AddRazorPages(); // host the UI
builder.Services.AddViteServices(new ViteOptions() // use vite development server
{
    PackageDirectory = "UI2",
    Server = new ViteServerOptions
    {
        AutoRun = builder.Configuration.GetValue<bool>("Vite:AutoRun"), // enable to autostart vite dev server
        Https = true,
    }
});

// Write schema files to disk for graphql-codegen
if (builder.Environment.IsDevelopment()) 
    builder.Services.AddSingleton<IHostedService, WriteSchemaFiles>();

/* App */
WebApplication app = builder.Build();

// Reset DB on every run
await app.RunMigrations();

app.UseSecurityHeaders();
app.UseHttpsRedirection();

// This is used during production to serve the UI resources
if (!app.Environment.IsDevelopment()) 
    app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// WebSockets are required to use websockets transport with GraphQL
app.UseWebSockets();

// Some utility endpoints for the UI
app.UseBffEndpoints();

// Map Default schema to endpoint
app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");

// UI is hosted by Razor page
app.MapRazorPages();

if (app.Environment.IsDevelopment())
    // Serve UI resources in development using Vite
    app.UseViteDevelopmentServer();

// Allow UI router to handle SPA requests
app.MapFallbackToPage("/Index");
app.Run();