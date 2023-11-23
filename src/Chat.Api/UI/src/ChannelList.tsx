import { A } from "@solidjs/router";
import { client } from "./data";
import { For, createResource } from "solid-js";
import { graphql } from "./generated";
import { useChannels } from "./data/useChannels";

export function ChannelsList() {
  const channels = useChannels();
  return (
    <ul class="menu p-4 w-80 min-h-full text-base-content">
      <For each={channels()} fallback={<li>Loading...</li>}>
        {(channel) => (
          <li>
            <A href={`/channels/${channel.id}`}>{channel.name}</A>
          </li>
        )}
      </For>
    </ul>
  );
}
