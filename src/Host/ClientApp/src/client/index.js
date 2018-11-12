import { ApolloClient } from 'apollo-client';
import { InMemoryCache } from 'apollo-cache-inmemory';
import { onError } from 'apollo-link-error';
import { ApolloLink } from 'apollo-link';
import { SignalrLink, Client } from '@fugu-fw/fugu-graphql-server-link';

const fuguClient = new Client("/hubs/graphql");
const fuguLink = new SignalrLink(fuguClient);

const client = new ApolloClient({
  cache: new InMemoryCache(),
  link: fuguLink
});

export default client;