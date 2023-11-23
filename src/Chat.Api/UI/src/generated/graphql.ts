/* eslint-disable */
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
};

export type Channel = {
  __typename?: 'Channel';
  description: Scalars['String']['output'];
  id: Scalars['Int']['output'];
  messages?: Maybe<Array<Message>>;
  name: Scalars['String']['output'];
};

export type ChannelEvent = {
  channelId: Scalars['Int']['output'];
  eventType: Scalars['String']['output'];
};

export type Message = {
  __typename?: 'Message';
  channelId: Scalars['Int']['output'];
  id: Scalars['Int']['output'];
  sender: Sender;
  text: Scalars['String']['output'];
  timestampMs: Scalars['String']['output'];
};

export type MessageChannelEvent = ChannelEvent & {
  __typename?: 'MessageChannelEvent';
  channelId: Scalars['Int']['output'];
  eventType: Scalars['String']['output'];
  message: Message;
};

export type Mutation = {
  __typename?: 'Mutation';
  addChannel: MutationChannel;
  channel: MutationChannel;
};


export type MutationAddChannelArgs = {
  description: Scalars['String']['input'];
  name: Scalars['String']['input'];
};


export type MutationChannelArgs = {
  id: Scalars['Int']['input'];
};

export type MutationChannel = {
  __typename?: 'MutationChannel';
  addMessage: Message;
  description: Scalars['String']['output'];
  id: Scalars['Int']['output'];
  name: Scalars['String']['output'];
};


export type MutationChannelAddMessageArgs = {
  text: Scalars['String']['input'];
};

export type Query = {
  __typename?: 'Query';
  channel: Channel;
  channels?: Maybe<Array<Channel>>;
};


export type QueryChannelArgs = {
  id: Scalars['Int']['input'];
};

export type Sender = {
  __typename?: 'Sender';
  avatarUrl: Scalars['String']['output'];
  id: Scalars['String']['output'];
  login: Scalars['String']['output'];
  name: Scalars['String']['output'];
};

export type Subscription = {
  __typename?: 'Subscription';
  channel_events?: Maybe<ChannelEvent>;
};


export type SubscriptionChannel_EventsArgs = {
  id: Scalars['Int']['input'];
};

export type ChannelsQueryVariables = Exact<{ [key: string]: never; }>;


export type ChannelsQuery = { __typename?: 'Query', channels?: Array<{ __typename?: 'Channel', id: number, name: string }> | null };


export const ChannelsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Channels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"channels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<ChannelsQuery, ChannelsQueryVariables>;