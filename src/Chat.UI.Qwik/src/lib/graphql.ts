import { createClient, SubscribePayload } from 'graphql-ws';

export interface GraphQL<T> {
    data: T;
    errors: any[];
    extensions: any[];
  }