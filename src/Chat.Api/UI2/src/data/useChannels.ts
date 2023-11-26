import { graphql } from "../generated";
import { useQuery } from "@urql/preact";

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
    }
}
`);

export function useChannels() {
  const [result] = useQuery({ 
    query: ChannelsQuery 
  });

  return result;
}

export function useChannel(id: number) {
  const [result] = useQuery({
    query: ChannelByIdQuery,
    variables: { id }
  });

  return result;
}