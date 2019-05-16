import { ApolloClient } from "apollo-client";
import { InMemoryCache } from "apollo-cache-inmemory";
import { onError } from "apollo-link-error";
import { ApolloLink } from "apollo-link";
import { RetryLink } from "apollo-link-retry";
import { TankaLink, TankaClient } from "@tanka/tanka-graphql-server-link";
import auth from "../auth";

var options = {
  reconnectAttempts: Infinity,
  reconnectInitialWaitMs: 5000,
  reconnectAdditionalWaitMs: 2000,
  connection: {
    accessTokenFactory: () => {
      var token = auth.getAccessToken();
      console.log(`AT: ${token}`);
      return token;
    }
  }
};

export default function clientFactory() {
  const serverClient = new TankaClient("/hubs/graphql", options);
  const serverLink = new TankaLink(serverClient);

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
      serverLink
    ]),
    cache: new InMemoryCache()
  });

  return client;
}
