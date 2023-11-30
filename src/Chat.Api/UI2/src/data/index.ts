import { Client, fetchExchange, subscriptionExchange } from "@urql/core";
import { cacheExchange } from "@urql/exchange-graphcache";

import { createClient as createWSClient } from "graphql-ws";
import { ChannelByIdQuery } from "./useChannels";

const wsClient = createWSClient({
  url: "wss://localhost:8000/graphql/ws",
  keepAlive: 10_000,
  retryAttempts: 10,
  lazy: false,
});

const client = new Client({
  url: "/graphql",
  exchanges: [
    cacheExchange({
      updates: {
        Subscription: {
          channel_events: (result, args, cache, info) => {
            
            if (result.channel_events["__typename"] !== "MessageChannelEvent") {
              return;
            }

            const message = result.channel_events["message"];
            const variables = {id: args.id};

            cache.updateQuery({query: ChannelByIdQuery, variables }, (data) => {
              if (data && data.channel) {
                if (!data.channel.messages) {
                  data.channel.messages = [];
                }

                data.channel.messages.push(message);
              }

              console.log("data", data)
              return data;
            });
          }
        }
      }
    }),
    fetchExchange,
    subscriptionExchange({
      forwardSubscription(request) {
        const input = { ...request, query: request.query || "" };
        return {
          subscribe(sink) {
            const unsubscribe = wsClient.subscribe(input, sink);
            return { unsubscribe };
          },
        };
      },
    }),
  ],
});

export { client };
