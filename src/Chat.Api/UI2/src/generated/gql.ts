/* eslint-disable */
import * as types from './graphql';
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';

/**
 * Map of all GraphQL operations in the project.
 *
 * This map has several performance disadvantages:
 * 1. It is not tree-shakeable, so it will include all operations in the project.
 * 2. It is not minifiable, so the string of a GraphQL query will be multiple times inside the bundle.
 * 3. It does not support dead code elimination, so it will add unused operations.
 *
 * Therefore it is highly recommended to use the babel or swc plugin for production.
 */
const documents = {
    "\n  mutation AddMessage($command: ChannelCommand!) {\n    execute(command: $command) {\n      __typename\n      ... on AddMessageResult {\n        message {\n          id\n          text\n          timestampMs\n          sender {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n": types.AddMessageDocument,
    "\n  query Channels {\n    channels {\n      id\n      name\n    }\n  }\n": types.ChannelsDocument,
    "\n  query ChannelById($id: Int!) {\n    channel(id: $id) {\n      id\n      name\n      description\n      messages {\n        id\n        text\n        timestampMs\n        sender {\n          id\n          name\n          login\n          avatarUrl\n        }\n      }\n    }\n  }\n": types.ChannelByIdDocument,
    "\n  subscription Events($id: Int!) {\n    channelEvents(id: $id) {\n      __typename\n      ... on MessageChannelEvent {\n        __typename\n        message {\n          __typename\n          id\n          text\n          timestampMs\n          sender {\n            __typename\n            id\n            name\n            avatarUrl\n            login\n          }\n        }\n      }\n    }\n  }\n": types.EventsDocument,
};

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 *
 *
 * @example
 * ```ts
 * const query = graphql(`query GetUser($id: ID!) { user(id: $id) { name } }`);
 * ```
 *
 * The query argument is unknown!
 * Please regenerate the types.
 */
export function graphql(source: string): unknown;

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation AddMessage($command: ChannelCommand!) {\n    execute(command: $command) {\n      __typename\n      ... on AddMessageResult {\n        message {\n          id\n          text\n          timestampMs\n          sender {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation AddMessage($command: ChannelCommand!) {\n    execute(command: $command) {\n      __typename\n      ... on AddMessageResult {\n        message {\n          id\n          text\n          timestampMs\n          sender {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query Channels {\n    channels {\n      id\n      name\n    }\n  }\n"): (typeof documents)["\n  query Channels {\n    channels {\n      id\n      name\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ChannelById($id: Int!) {\n    channel(id: $id) {\n      id\n      name\n      description\n      messages {\n        id\n        text\n        timestampMs\n        sender {\n          id\n          name\n          login\n          avatarUrl\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query ChannelById($id: Int!) {\n    channel(id: $id) {\n      id\n      name\n      description\n      messages {\n        id\n        text\n        timestampMs\n        sender {\n          id\n          name\n          login\n          avatarUrl\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  subscription Events($id: Int!) {\n    channelEvents(id: $id) {\n      __typename\n      ... on MessageChannelEvent {\n        __typename\n        message {\n          __typename\n          id\n          text\n          timestampMs\n          sender {\n            __typename\n            id\n            name\n            avatarUrl\n            login\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  subscription Events($id: Int!) {\n    channelEvents(id: $id) {\n      __typename\n      ... on MessageChannelEvent {\n        __typename\n        message {\n          __typename\n          id\n          text\n          timestampMs\n          sender {\n            __typename\n            id\n            name\n            avatarUrl\n            login\n          }\n        }\n      }\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;