import { component$ } from "@builder.io/qwik";
import {
  type DocumentHead,
  routeLoader$,
  routeAction$,
  Form,
} from "@builder.io/qwik-city";
import { useAuthSession } from "~/routes/plugin@auth";
import { type Session } from "@auth/core/types";
import { addMessage, fetchChannelAndMessages } from "~/lib/api";

export const useInitialMessages = routeLoader$(async (event) => {
  const id = parseInt(event.params.id);
  const session: Session | null = event.sharedMap.get("session");

  if (!session) {
    throw event.error(401, "Unauthorized");
  }

  return await fetchChannelAndMessages(id, session.accessToken);
});

export const useAddMessage = routeAction$(async (data, event) => {
  const session = event.sharedMap.get("session") as {
    accessToken: string;
  };
  const id = parseInt(event.params.id);
  return await addMessage(data, session.accessToken, id);
});

export default component$(() => {
  const addMessage = useAddMessage();
  const session = useAuthSession();
  const channel = useInitialMessages();

  return (
    <div>
      <div class="p-6 h-auto">
        <p class="text-lg">{channel.value.name}</p>
      </div>
      <div class="p-6 pt-0 overflow-y-auto w-6/12" style={{ height: "85vh" }}>
        {channel.value.messages.map((message) => (
          <div
            class={[
              "chat",
              message.sender.sub === session.value?.user.sub
                ? "chat-end"
                : "chat-start",
            ]}
            key={message.id}
          >
            <div class="chat-image">
              <div class="avatar placeholder">
                <div class="bg-neutral-focus text-neutral-content rounded-full w-12 ring-1 ring-secondary">
                  <span>
                    {message.sender.name.split(" ").map((v) => v[0] ?? "?")}
                  </span>
                </div>
              </div>
            </div>
            <div class="chat-bubble chat-bubble-secondary">
              <div>
                <time class="text-sm opacity-60">
                  {message.timestamp.toLocaleString()}
                </time>
              </div>
              <div class="text-lg">{message.text}</div>
            </div>
          </div>
        ))}
      </div>
      <div class="absolute bottom-0 p-12 h-auto">
        <Form
          action={addMessage}
          onSubmitCompleted$={(_, form) => {
            form.reset();
          }}
        >
          <div class="grid grid-cols-12 h-16 items-end ">
            <div class="form-control col-span-11">
              <label class="label">
                <span class="label-text">Message</span>
              </label>
              <input
                name="message"
                type="text"
                placeholder="Type here"
                class="input input-bordered input-accent"
              />
            </div>
            <button
              class="btn btn-outline btn-secondary rounded-sm w-16 h-12 col-span-1"
              type="submit"
            >
              Send
            </button>
          </div>
        </Form>
      </div>
    </div>
  );
});

export const head: DocumentHead = ({ resolveValue }) => {
  const channel = resolveValue(useInitialMessages);
  return {
    title: `${channel.name} - Tanka Chat - Qwik`,
    meta: [
      {
        name: "description",
        content: channel.description,
      },
    ],
  };
};
