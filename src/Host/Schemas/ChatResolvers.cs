﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using fugu.graphql.resolvers;
using Microsoft.AspNetCore.Hosting.Internal;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.Host.Schemas
{
    public class ChatResolvers : ResolverMap
    {
        public ChatResolvers(ChatResolverService resolver)
        {
            // roots
            this["Query"] = new FieldResolverMap
            {
                {"channels", resolver.Channels},
                {"channel", resolver.Channel}
            };

            this["Mutation"] = new FieldResolverMap();
            this["Subscription"] = new FieldResolverMap();

            // domain
            this["Channel"] = new FieldResolverMap
            {
                {"id", PropertyOf<Channel>(c => c.Id)},
                {"name", PropertyOf<Channel>(c => c.Name)},
                {"members", PropertyOf<Channel>(c => c.Members)}
            };

            this["Member"] = new FieldResolverMap()
            {
                {"id", PropertyOf<Member>(c => c.Id)},
                {"name", PropertyOf<Member>(c => c.Name)},
            };

            this["Message"] = new FieldResolverMap
            {
                {"content", PropertyOf<Message>(m => m.Content)}
            };
        }
    }

    public class ChatResolverService
    {
        public Task<IResolveResult> Channels(ResolverContext context)
        {
            var channels = new[]
            {
                new Channel
                {
                    Id = 1,
                    Name = "General"
                }
            };

            return Task.FromResult(As(channels));
        }

        public Task<IResolveResult> Channel(ResolverContext context)
        {
            var id = context.GetArgument<string>("id");
            var channel = new Channel
            {
                Id = int.Parse(id), //todo(pekka): fix bug in the GetArgument
                Name = "General"
            };

            return Task.FromResult(As(channel));
        }
    }

    public class Channel
    {
        public int Id { get; set; }

        public string Name { get; set; }
         
        public ConcurrentBag<Member> Members { get; set; } = new ConcurrentBag<Member>();
    }

    public class Member
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }


    public class Message
    {
        public string Content { get; set; }
    }
}