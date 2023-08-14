import { createClient as createClientInternal, SubscribePayload } from 'graphql-ws';
import WebSocket from 'ws';

export interface GraphQL<T> {
  data: T;
  errors: any[];
  extensions: any[];
}

export function createClient(accessToken: string) {
  const client = createClientInternal({
    url: `wss://localhost:8000/graphql/ws?a=${accessToken}`,
    webSocketImpl: WebSocket,
  });

  return client;
}