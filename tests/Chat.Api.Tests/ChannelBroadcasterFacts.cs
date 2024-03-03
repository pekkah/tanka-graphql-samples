using Tanka.GraphQL.Samples.Chat.Api.Schema;

namespace Tanka.GraphQL.Samples.Chat.Api.Tests;

public class ChannelBroadcasterFacts
{
    [Fact]
    public async Task Subscribe()
    {
        /* Given */
        var sut = new ChannelBroadcaster();
        var cts = new CancellationTokenSource();
        var expected = new MessageChannelEvent(
            1,
            "test",
            new Schema.Message { TimestampMs = "1", Text = "test", ChannelId = 1 }
            );

        /* When */
        var stream = sut.Subscribe(cts.Token);
        await sut.Publish(expected, CancellationToken.None);
        var actual = await stream.FirstAsync();
        await cts.CancelAsync();

        /* Then */
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Subscribe_multiple()
    {
        /* Given */
        var sut = new ChannelBroadcaster();
        var cts = new CancellationTokenSource();
        var expected = new MessageChannelEvent(
            1,
            "test",
            new Schema.Message { TimestampMs = "1", Text = "test", ChannelId = 1 }
        );

        /* When */
        var streamOne = sut.Subscribe(cts.Token);
        var streamTwo = sut.Subscribe(cts.Token);
        await sut.Publish(expected, CancellationToken.None);
        var actualOne = await streamOne.FirstAsync();
        var actualTwo = await streamTwo.FirstAsync();
        await cts.CancelAsync();

        /* Then */
        Assert.Equal(expected, actualOne);
        Assert.Equal(expected, actualTwo);
    }
}