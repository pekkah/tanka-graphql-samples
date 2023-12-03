import { graphql } from "../generated";
import { useQuery, useSubscription } from "@urql/preact";
import {
  Channel,
  EventsSubscription,
  Message,
  MessageChannelEvent,
} from "../generated/graphql";
import { useState } from "preact/hooks";

const ChannelsQuery = graphql(`
  query Channels {
    channels {
      id
      name
    }
  }
`);

export const ChannelByIdQuery = graphql(`
  query ChannelById($id: Int!) {
    channel(id: $id) {
      id
      name
      description
      messages {
        id
        text
        timestampMs
        sender {
          id
          name
          login
          avatarUrl
        }
      }
    }
  }
`);

export function useChannels() {
  const [result] = useQuery({
    query: ChannelsQuery,
  });

  return result;
}

export function useChannel(id: number) {
  const [result] = useQuery({
    query: ChannelByIdQuery,
    variables: { id },
  });

  return result;
}

const EventsSubscriptionQuery = graphql(`
  subscription Events($id: Int!) {
    channel_events(id: $id) {
      __typename
      ... on MessageChannelEvent {
        __typename
        message {
          __typename
          id
          text
          timestampMs
          sender {
            __typename
            id
            name
            avatarUrl
            login
          }
        }
      }
    }
  }
`);

export function useChannelWithNewMessages(id: number) {
  const initialQuery = useQuery({
    query: ChannelByIdQuery,
    variables: { id },
  });
  
  const [{error: subscriptionError, fetching: subscriptionConnected}] = useSubscription({
      pause: initialQuery[0].fetching,
      query: EventsSubscriptionQuery,
      variables: {
        id,
      },
    }
  );

  return {
    ...initialQuery[0],
    subscriptionError,
    subscriptionConnected
  };
}
