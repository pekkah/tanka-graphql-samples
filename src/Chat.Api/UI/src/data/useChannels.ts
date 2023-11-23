import { graphql } from "../generated";
import { createQuery } from ".";
import { createComputed, createSignal } from "solid-js";
import { Channel } from "../generated/graphql";

const ChannelsQuery = graphql(`
  query Channels {
    channels {
      id
      name
    }
  }
`);

export function useChannels() {
  const [channels, setChannels] = createSignal<Partial<Channel>[]>([]);
  const result = createQuery(
    () => ChannelsQuery,
    () => ({})
  );

  createComputed(() => {
    if (result()) {
      setChannels(result()?.data?.channels ?? []);
    }
  });

  return channels;
}
