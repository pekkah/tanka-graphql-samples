using System.Threading.Tasks;
using fugu.graphql.samples.Host.Logic.Domain;
using Xunit;

namespace Host.Facts
{
    public class ChatFacts
    {
        [Fact]
        public async Task Post_Message_()
        {
            /* Given */
            var inputMessage = new InputMessage()
            {
                Content = "123"
            };
            var channelId = 1;
            var sut = new Chat();


            /* When */
            await sut.PostMessageAsync(channelId, inputMessage);

            /* Then */
            var messages = sut.GetMessagesAsync(channelId);
            Assert.Single(messages, m => m.Content == inputMessage.Content);
        }
    }
}