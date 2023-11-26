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

export function useChannels() {
  const [result] = useQuery({ query: ChannelsQuery });
  return result;
}
