// @ts-nocheck
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type Scalars = {
    Boolean: boolean,
    Float: number,
    ID: string,
    Int: number,
    String: string,
}

export interface Channel {
    description: Scalars['String']
    id: Scalars['Int']
    messages: (Message[] | null)
    name: Scalars['String']
    __typename: 'Channel'
}

export interface Message {
    channelId: Scalars['Int']
    id: Scalars['Int']
    text: Scalars['String']
    timestampMs: Scalars['String']
    __typename: 'Message'
}

export interface Mutation {
    addChannel: MutationChannel
    channel: MutationChannel
    __typename: 'Mutation'
}

export interface MutationChannel {
    addMessage: Message
    description: Scalars['String']
    id: Scalars['Int']
    name: Scalars['String']
    __typename: 'MutationChannel'
}

export interface Query {
    channel: Channel
    channels: (Channel[] | null)
    __typename: 'Query'
}

export interface ChannelGenqlSelection{
    description?: boolean | number
    id?: boolean | number
    messages?: MessageGenqlSelection
    name?: boolean | number
    __typename?: boolean | number
    __scalar?: boolean | number
}

export interface MessageGenqlSelection{
    channelId?: boolean | number
    id?: boolean | number
    text?: boolean | number
    timestampMs?: boolean | number
    __typename?: boolean | number
    __scalar?: boolean | number
}

export interface MutationGenqlSelection{
    addChannel?: (MutationChannelGenqlSelection & { __args: {name: Scalars['String'], description: Scalars['String']} })
    channel?: (MutationChannelGenqlSelection & { __args: {id: Scalars['Int']} })
    __typename?: boolean | number
    __scalar?: boolean | number
}

export interface MutationChannelGenqlSelection{
    addMessage?: (MessageGenqlSelection & { __args: {text: Scalars['String']} })
    description?: boolean | number
    id?: boolean | number
    name?: boolean | number
    __typename?: boolean | number
    __scalar?: boolean | number
}

export interface QueryGenqlSelection{
    channel?: (ChannelGenqlSelection & { __args: {id: Scalars['Int']} })
    channels?: ChannelGenqlSelection
    __typename?: boolean | number
    __scalar?: boolean | number
}


    const Channel_possibleTypes: string[] = ['Channel']
    export const isChannel = (obj?: { __typename?: any } | null): obj is Channel => {
      if (!obj?.__typename) throw new Error('__typename is missing in "isChannel"')
      return Channel_possibleTypes.includes(obj.__typename)
    }
    


    const Message_possibleTypes: string[] = ['Message']
    export const isMessage = (obj?: { __typename?: any } | null): obj is Message => {
      if (!obj?.__typename) throw new Error('__typename is missing in "isMessage"')
      return Message_possibleTypes.includes(obj.__typename)
    }
    


    const Mutation_possibleTypes: string[] = ['Mutation']
    export const isMutation = (obj?: { __typename?: any } | null): obj is Mutation => {
      if (!obj?.__typename) throw new Error('__typename is missing in "isMutation"')
      return Mutation_possibleTypes.includes(obj.__typename)
    }
    


    const MutationChannel_possibleTypes: string[] = ['MutationChannel']
    export const isMutationChannel = (obj?: { __typename?: any } | null): obj is MutationChannel => {
      if (!obj?.__typename) throw new Error('__typename is missing in "isMutationChannel"')
      return MutationChannel_possibleTypes.includes(obj.__typename)
    }
    


    const Query_possibleTypes: string[] = ['Query']
    export const isQuery = (obj?: { __typename?: any } | null): obj is Query => {
      if (!obj?.__typename) throw new Error('__typename is missing in "isQuery"')
      return Query_possibleTypes.includes(obj.__typename)
    }
    