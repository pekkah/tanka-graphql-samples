using System;

namespace tanka.graphql.samples.messages.host.logic
{
    public partial class Message
    {
        public Message()
        {
            Timestamp = DateTimeOffset.UtcNow;
            Id = -1;
            ChannelId = -1;
            Content = string.Empty;
            From = string.Empty;
        }
    }
}