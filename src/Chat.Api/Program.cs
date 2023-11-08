using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Channels;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Samples.Chat;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Vite.AspNetCore;
using Vite.AspNetCore.Extensions;

using Channel = System.Threading.Channels.Channel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddPooledDbContextFactory<ChatContext>(
        options => options.UseSqlite(
            "Data Source=Chat.db;Cache=Shared"
        )
    );

builder.Services.AddSingleton<IChannelEvents, ChannelEvents>();
builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        options.AddGeneratedTypes(types =>
        {
            types.AddTankaGraphQLSamplesChatTypes();
        });

        //TODO: Add subscription support to the code generator
        options.Configure(configure =>
        {
            // Configure subscription types
            ExecutableSchemaBuilder builder = configure.Builder;

            builder.Add("""
                        interface ChannelEvent {
                            channelId: Int!
                            eventType: String!
                        }

                        type MessageChannelEvent implements ChannelEvent
                        """);

            builder.Add(new ObjectResolversConfiguration(
                "MessageChannelEvent",
                new FieldsWithResolvers
                {
                    { "channelId: Int!", (MessageChannelEvent objectValue) => objectValue.ChannelId },
                    { "eventType: String!", (MessageChannelEvent objectValue) => objectValue.EventType },
                    { "message: Message!", (MessageChannelEvent objectValue) => objectValue.Message }
                }));

            builder.Add("Subscription",
                new FieldsWithResolvers
                {
                    {
                        "channel_events(id: Int!): ChannelEvent", (ResolverContext context) =>
                        {
                            context.ResolvedValue = context.ObjectValue;
                            context.ResolveAbstractType = (definition, value) => value switch
                            {
                                MessageChannelEvent => context.Schema.GetRequiredNamedType<ObjectDefinition>("MessageChannelEvent"),
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
                            var id = context.GetArgument<int>("id");
                            context.ResolvedValue = events.Subscribe(id, unsubscribe);
                            return default;
                        })
                    }
                });
        });
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = "github";
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/signin";
    })
    .AddGitHub("github", options =>
    {
        options.ClientId = builder.Configuration["GitHub:ClientId"];
        options.ClientSecret = builder.Configuration["GitHub:ClientSecret"];
        options.Scope.Add("user:email");

        options.ClaimActions.MapAll();
    });

builder.Services.AddRazorPages();
builder.Services.AddViteServices(new ViteOptions()
{
    PackageDirectory = "UI",
    Server = new ViteServerOptions()
    {
        Https = true
    }
});


WebApplication app = builder.Build();
app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    IDbContextFactory<ChatContext> dbFactory = app.Services.GetRequiredService<IDbContextFactory<ChatContext>>();
    await using ChatContext db = await dbFactory.CreateDbContextAsync();
    await db.Database.EnsureDeletedAsync();
    try
    {
        await db.Database.EnsureCreatedAsync();

        await db.Channels.AddRangeAsync(
            new Tanka.GraphQL.Samples.Chat.Channel { Name = "General", Description = "" },
            new Tanka.GraphQL.Samples.Chat.Channel { Name = "Tanka", Description = "" }
        );

        await db.SaveChangesAsync();
    }
    catch (Exception x)
    {
        app.Services.GetRequiredService<ILogger<Program>>().LogError(x, "Failed to initialize database");
        throw;
    }
}

app.UseWebSockets();

app.MapGet("/signin", async context => await context.ChallengeAsync("github", new OAuthChallengeProperties()
{
    RedirectUri = "/"
}));
app.MapGet("/signout", async context => await context.SignOutAsync("Cookies", new AuthenticationProperties()
{
    RedirectUri = "/"
}));
app.MapGet("/session", async context =>
{
    if (context.User.Identity?.IsAuthenticated == false)
    {
        await context.Response.WriteAsJsonAsync(new
        {
            IsAuthenticated = false
        });
    }
    else
    {
        await context.Response.WriteAsJsonAsync(new
        {
            IsAuthenticated = true,
            Name = context.User.FindFirstValue("name"),
            AvatarUrl = context.User.FindFirstValue("avatar_url"),
            Login = context.User.FindFirstValue("login"),
        });
    }
});


app.MapTankaGraphQL("/graphql", "Default");


app.MapRazorPages();


if (app.Environment.IsDevelopment())
{
    app.UseViteDevMiddleware();
}

app.Run();

public interface IChannelEvents
{
    ValueTask Publish<T>(T channelEvent) where T : ChannelEvent;

    IAsyncEnumerable<ChannelEvent> Subscribe(int channelId, CancellationToken cancellationToken);
}

public class ChannelEvents : IChannelEvents
{
    private readonly ConcurrentDictionary<int, SimpleAsyncSubject<ChannelEvent>> _channels = new();

    public ValueTask Publish<T>(T channelEvent) where T : ChannelEvent
    {
        int channelId = channelEvent.ChannelId;
        SimpleAsyncSubject<ChannelEvent> channel =
            _channels.GetOrAdd(channelId, id => new ConcurrentSimpleAsyncSubject<ChannelEvent>());

        return channel.OnNextAsync(channelEvent);
    }

    public IAsyncEnumerable<ChannelEvent> Subscribe(int channelId, CancellationToken cancellationToken)
    {
        //todo: channel with channelId exists
        SimpleAsyncSubject<ChannelEvent> channel =
            _channels.GetOrAdd(channelId, id => new ConcurrentSimpleAsyncSubject<ChannelEvent>());

         return new Subscription(channel).AsAsyncEnumerable(cancellationToken);
    }
}

public abstract record ChannelEvent(int ChannelId, string EventType);

public record MessageChannelEvent
    (int ChannelId, string EventType, Message Message) : ChannelEvent(ChannelId, EventType);

public class Subscription : IAsyncObserver<ChannelEvent>
{
    private readonly Channel<ChannelEvent> _channel =
        Channel.CreateUnbounded<ChannelEvent>(
            new UnboundedChannelOptions { SingleWriter = true, SingleReader = true });

    private readonly IAsyncObservable<ChannelEvent> _observable;

    public Subscription(IAsyncObservable<ChannelEvent> observable)
    {
        _observable = observable;
    }

    protected ChannelReader<ChannelEvent> Reader => _channel.Reader;

    protected ChannelWriter<ChannelEvent> Writer => _channel.Writer;

    public ValueTask OnNextAsync(ChannelEvent value)
    {
        return Writer.WriteAsync(value);
    }

    public ValueTask OnErrorAsync(Exception error)
    {
        Writer.Complete(error);
        return default(ValueTask);
    }

    public ValueTask OnCompletedAsync()
    {
        Writer.Complete();
        return default(ValueTask);
    }

    public async IAsyncEnumerable<ChannelEvent> AsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            yield break;

        IAsyncDisposable? disposable = await _observable.SubscribeAsync(this);

        cancellationToken.Register(() =>
        {
            disposable.DisposeAsync();
        });

        // ReSharper disable once MethodSupportsCancellation
        await foreach (ChannelEvent item in Reader.ReadAllAsync(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return item;
        }

        await disposable.DisposeAsync();
    }
}