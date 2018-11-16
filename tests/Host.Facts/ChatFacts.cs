using System;
using System.Threading.Tasks;
using fugu.graphql.samples.Host.Logic.Domain;
using Xunit;

namespace Host.Facts
{
    public class ChatFacts
    {
        [Fact]
        public async Task Post_message()
        {
            /* Given */
            var inputMessage = new InputMessage()
            {
                Content = "123"
            };
            var sut = new Chat();
            var channel = await sut.CreateChannelAsync(new InputChannel()
            {
                Name = "General"
            });


            /* When */
            await sut.PostMessageAsync(channel.Id, inputMessage);

            /* Then */
            var messages = await sut.GetMessagesAsync(channel.Id);
            Assert.Single(messages, m => m.Content == inputMessage.Content);
        }

        [Fact]
        public async Task Post_message_should_fail_if_no_channel()
        {
            /* Given */
            /* Given */
            var inputMessage = new InputMessage()
            {
                Content = "123"
            };
            var channelId = 1;
            var sut = new Chat();


            /* When */
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                ()=> sut.PostMessageAsync(channelId, inputMessage));

            /* Then */
            Assert.NotNull(exception.Message);
        }

        [Fact]
        public async Task Create_channel()
        {
            /* Given */
            var sut = new Chat();

            /* When */
            var channel = await sut.CreateChannelAsync(new InputChannel() {Name = "General"});

            /* Then */
            Assert.NotNull(channel);
            Assert.NotNull(await sut.GetChannelAsync(channel.Id));
        }
    }
}