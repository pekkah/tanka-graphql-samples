import { ApolloClient } from 'apollo-client';
import { InMemoryCache } from 'apollo-cache-inmemory';
import { onError } from 'apollo-link-error';
import { ApolloLink } from 'apollo-link';
import { TankaLink, TankaClient } from '@tanka/tanka-graphql-server-link';

var options = {
  reconnectAttempts: Infinity,
  reconnectInitialWaitMs: 5000,
  reconnectAdditionalWaitMs: 2000
};

const serverClient = new TankaClient("/hubs/graphql", options);
const serverLink = new TankaLink(serverClient);

const client = new ApolloClient({
  connectToDevTools: true,
  link: ApolloLink.from([
    onError(({ graphQLErrors, networkError }) => {
      if (graphQLErrors)
        graphQLErrors.map(({ message, locations, path }) =>
          console.log(
            `[GraphQL error]: Message: ${message}, Location: ${locations}, Path: ${path}`,
          ),
        );
      if (networkError) console.log(`[Network error]: ${networkError}`);
    }),
    serverLink
  ]),
  cache: new InMemoryCache()
});

export default client;