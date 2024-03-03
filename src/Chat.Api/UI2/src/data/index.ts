import { Client, fetchExchange, subscriptionExchange } from "@urql/core";
import { cacheExchange } from "@urql/exchange-graphcache";

import { createClient as createWSClient } from "graphql-ws";
import { ChannelByIdQuery } from "./useChannels";

const host = window.location.host;
const wsClient = createWSClient({
  url: `wss://${host}/graphql/ws`,
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
          channelEvents: (result, args, cache, info) => {
            
            if (result.channelEvents["__typename"] !== "MessageChannelEvent") {
              console.log(result.channelEvents);
              return;
            }

            const message = result.channelEvents["message"];
            const variables = {id: args.id};

            cache.updateQuery({query: ChannelByIdQuery, variables }, (data) => {
              if (data && data.channel) {
                if (!data.channel.messages) {
                  data.channel.messages = [];
                }

                data.channel.messages.push(message);
              }

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
