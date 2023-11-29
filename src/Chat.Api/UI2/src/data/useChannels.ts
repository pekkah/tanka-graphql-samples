import { graphql } from "../generated";
import { useQuery, useSubscription } from "@urql/preact";
import { EventsSubscription, Message, MessageChannelEvent } from "../generated/graphql";

const ChannelsQuery = graphql(`
  query Channels {
    channels {
      id
      name
    }
  }
`);

const ChannelByIdQuery = graphql(`
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

export function useNewMessages(
  id: number,
  initialQuery: { fetching: boolean, data: Partial<Message>[] | undefined },
) {
  const [result] = useSubscription(
    {
      pause: initialQuery.fetching,
      query: EventsSubscriptionQuery,
      variables: {
        id,
      },
    },
    handle
  );

  function handle(prev: Partial<Message>[] = initialQuery.data, event: EventsSubscription){
    if (event.channel_events.__typename !== 'MessageChannelEvent') return prev;

    const messageEvent = event.channel_events as MessageChannelEvent;
    return [...prev, messageEvent.message as Message]
  }

  return result.data || initialQuery.data;
}
