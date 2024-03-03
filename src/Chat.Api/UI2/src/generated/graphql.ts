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

export type AddChannelCommand = {
  description: Scalars['String']['input'];
  name: Scalars['String']['input'];
};

export type AddChannelResult = {
  __typename?: 'AddChannelResult';
  channel: Channel;
};

export type AddMessageCommand = {
  channelId: Scalars['Int']['input'];
  content: Scalars['String']['input'];
};

export type AddMessageResult = {
  __typename?: 'AddMessageResult';
  message: Message;
};

export type Channel = {
  __typename?: 'Channel';
  description: Scalars['String']['output'];
  id: Scalars['Int']['output'];
  messages?: Maybe<Array<Message>>;
  name: Scalars['String']['output'];
};

export type ChannelCommand =
  { addChannel: AddChannelCommand; addMessage?: never; }
  |  { addChannel?: never; addMessage: AddMessageCommand; };

export type CommandResult = AddChannelResult | AddMessageResult;

export type IChannelEvent = {
  channelId: Scalars['Int']['output'];
};

export type Message = {
  __typename?: 'Message';
  channelId: Scalars['Int']['output'];
  id: Scalars['Int']['output'];
  sender: Sender;
  text: Scalars['String']['output'];
  timestampMs: Scalars['String']['output'];
};

export type MessageChannelEvent = IChannelEvent & {
  __typename?: 'MessageChannelEvent';
  channelId: Scalars['Int']['output'];
  message: Message;
};

export type Mutation = {
  __typename?: 'Mutation';
  execute: CommandResult;
};


export type MutationExecuteArgs = {
  command: ChannelCommand;
};

export type Query = {
  __typename?: 'Query';
  channel?: Maybe<Channel>;
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
  channelEvents: IChannelEvent;
};


export type SubscriptionChannelEventsArgs = {
  id: Scalars['Int']['input'];
};

export type AddMessageMutationVariables = Exact<{
  command: ChannelCommand;
}>;


export type AddMessageMutation = { __typename?: 'Mutation', execute: { __typename: 'AddChannelResult' } | { __typename: 'AddMessageResult', message: { __typename?: 'Message', id: number, text: string, timestampMs: string, sender: { __typename?: 'Sender', id: string, name: string } } } };

export type ChannelsQueryVariables = Exact<{ [key: string]: never; }>;


export type ChannelsQuery = { __typename?: 'Query', channels?: Array<{ __typename?: 'Channel', id: number, name: string }> | null };

export type ChannelByIdQueryVariables = Exact<{
  id: Scalars['Int']['input'];
}>;


export type ChannelByIdQuery = { __typename?: 'Query', channel?: { __typename?: 'Channel', id: number, name: string, description: string, messages?: Array<{ __typename?: 'Message', id: number, text: string, timestampMs: string, sender: { __typename?: 'Sender', id: string, name: string, login: string, avatarUrl: string } }> | null } | null };

export type EventsSubscriptionVariables = Exact<{
  id: Scalars['Int']['input'];
}>;


export type EventsSubscription = { __typename?: 'Subscription', channelEvents: { __typename: 'MessageChannelEvent', message: { __typename: 'Message', id: number, text: string, timestampMs: string, sender: { __typename: 'Sender', id: string, name: string, avatarUrl: string, login: string } } } };


export const AddMessageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AddMessage"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"command"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ChannelCommand"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"execute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"Variable","name":{"kind":"Name","value":"command"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AddMessageResult"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"text"}},{"kind":"Field","name":{"kind":"Name","value":"timestampMs"}},{"kind":"Field","name":{"kind":"Name","value":"sender"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<AddMessageMutation, AddMessageMutationVariables>;
export const ChannelsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Channels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"channels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<ChannelsQuery, ChannelsQueryVariables>;
export const ChannelByIdDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ChannelById"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"channel"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"messages"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"text"}},{"kind":"Field","name":{"kind":"Name","value":"timestampMs"}},{"kind":"Field","name":{"kind":"Name","value":"sender"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"login"}},{"kind":"Field","name":{"kind":"Name","value":"avatarUrl"}}]}}]}}]}}]}}]} as unknown as DocumentNode<ChannelByIdQuery, ChannelByIdQueryVariables>;
export const EventsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"Events"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"channelEvents"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MessageChannelEvent"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"Field","name":{"kind":"Name","value":"message"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"text"}},{"kind":"Field","name":{"kind":"Name","value":"timestampMs"}},{"kind":"Field","name":{"kind":"Name","value":"sender"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"avatarUrl"}},{"kind":"Field","name":{"kind":"Name","value":"login"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<EventsSubscription, EventsSubscriptionVariables>;