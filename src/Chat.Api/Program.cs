using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Samples.Chat;
using Tanka.GraphQL.Samples.Chat.Api;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

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

app.UseHttpsRedirection();
app.UseRouting();

app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
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