﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class Messages
    {
        private readonly EventChannel<Message> _messageAdded;
        private readonly List<Message> _messages = new List<Message>();

        private int _nextMessageId = 1;

        public Messages()
        {
            _messageAdded = new EventChannel<Message>();
        }

        public async Task<Message> PostMessageAsync(int channelId, string from, string profileUrl,
            InputMessage inputMessage)
        {
            var message = new Message
            {
                Id = NextId(),
                Content = inputMessage.Content,
                ChannelId = channelId,
                From = from,
                ProfileUrl = profileUrl
            };
            _messages.Add(message);

            await _messageAdded.WriteAsync(message);
            return message;
        }

        public Task<IEnumerable<Message>> GetMessagesAsync(int channelId, int latest = 100)
        {
            return Task.FromResult(_messages.AsEnumerable());
        }

        public ISubscriberResult Join(int channelId, CancellationToken unsubscribe)
        {
            var result = _messageAdded.Subscribe(unsubscribe);
            return result;
        }

        private int NextId()
        {
            return _nextMessageId++;
        }
    }

    public class InputMessage : IReadFromObjectDictionary
    {
        public string Content { get; set; }

        public void Read(IReadOnlyDictionary<string, object> source)
        {
            Content = source.GetValue<string>("content", null);
        }
    }

    public class Message
    {
        public Message()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset? Timestamp { get; set; }

        public int Id { get; set; }

        public string Content { get; set; }

        public int ChannelId { get; set; }

        public string From { get; set; }

        public string ProfileUrl { get; set; }
    }
}