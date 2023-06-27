import { type Session } from "@auth/core/types";
import {
  type QRL,
  Slot,
  component$,
  createContextId,
  useContextProvider,
  useStore,
  $,
  useContext,
  useTask$,
} from "@builder.io/qwik";
import { server$ } from "@builder.io/qwik-city";
import { fetchChannels } from "~/lib/api";
import type { Message, Channel } from "~/lib/types";
import { useAuthSession } from "~/routes/plugin@auth";

export const ChatContextId = createContextId<ChatContext>("ChatContext");
export const useChatContext = () => useContext(ChatContextId);

export interface ChatContext {
  messages: Message[];
  channels: Channel[];
  current?: Channel;
  selectChannel: QRL<(this: ChatContext, id: number) => void>;
  addMessages: QRL<(this: ChatContext, messages: Message[]) => void>;
}

const initialChannels = server$(async function () {
  const session: Session | null = this.sharedMap.get("session");

  if (!session?.accessToken) {
    throw new Error("Unauthorized");
  }
  return await fetchChannels(session.accessToken);
});

export default component$(() => {
  const session = useAuthSession();
  const context = useStore<ChatContext>({
    messages: [],
    channels: [],
    selectChannel: $(function (this: ChatContext, id: number) {
      const channel = this.channels.find((channel) => channel.id === id);
      if (!channel) {
        throw new Error(`Channel ${id} not found`);
      }

      this.current = channel;
      this.messages = channel.messages;
    }),
    addMessages: $(function (this: ChatContext, messages: Message[]) {
      console.log("addMessages", messages);
      if (this.current) this.current.messages.push(...messages);
    }),
  });

  useContextProvider(ChatContextId, context);

  useTask$(async function ({ track }) {
    const sessionValue = track(() => session);

    if (sessionValue.value) {
      const channels = await initialChannels();
      context.channels = channels;
    }
  });

  return <Slot />;
});
