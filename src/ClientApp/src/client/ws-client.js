import { ApolloClient } from "apollo-client";
import { InMemoryCache } from "apollo-cache-inmemory";
import { onError } from "apollo-link-error";
import { ApolloLink } from "apollo-link";
import { RetryLink } from "apollo-link-retry";
import { WebSocketLink } from "apollo-link-ws";
import { SubscriptionClient } from "subscriptions-transport-ws";
import auth from "../auth";

export default function clientFactory() {
  const host = window.CONFIG.gw.replace("https", "wss").replace("http", "ws");
  const wsClient = new SubscriptionClient(`${host}/api/graphql`, {
    reconnect: true,
    connectionParams: () => ({
      authorization: auth.getAccessToken()
    })
  });

  const wsLink = new WebSocketLink(wsClient);

  const client = new ApolloClient({
    connectToDevTools: true,
    link: ApolloLink.from([
      onError(({ graphQLErrors, networkError }) => {
        if (graphQLErrors)
          graphQLErrors.map(({ message, locations, path }) =>
            console.log(
              `[GraphQL error]: Message: ${message}, Location: ${locations}, Path: ${path}`
            )
          );
        if (networkError) console.log(`[Network error]: ${networkError}`);
      }),
      new RetryLink(),
      wsLink
    ]),
    cache: new InMemoryCache()
  });

  return client;
}