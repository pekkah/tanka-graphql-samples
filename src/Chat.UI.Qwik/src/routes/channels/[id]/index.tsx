import {
  component$,
  useTask$,
  useVisibleTask$,
  $,
  useSignal,
} from "@builder.io/qwik";
import {
  server$,
  useLocation,
} from "@builder.io/qwik-city";
import { useAuthSession } from "~/routes/plugin@auth";
import { type Session } from "@auth/core/types";
import { addMessage, fetchChannelAndMessages } from "~/lib/api";
import { useChatContext } from "~/components/ChatContext";
import { createClient } from "~/lib/graphql";
import { type Client as GraphQLClient } from "graphql-ws";
import type { MessageChannelEvent } from "~/lib/types";

const addMessageServer = server$(async function (text: string) {
  const session: Session | null = this.sharedMap.get("session");

  if (!session?.accessToken) {
    throw new Error("Unauthorized");
  }

  return await addMessage(text, session.accessToken, parseInt(this.params.id));
});

const getChannel = server$(async function () {
  const session: Session | null = this.sharedMap.get("session");

  if (!session?.accessToken) {
    throw new Error("Unauthorized");
  }

  return await fetchChannelAndMessages(
    parseInt(this.params.id),
    session.accessToken
  );
});

const streamMessages = server$(async function* () {
  const session: Session | null = this.sharedMap.get("session");

  if (!session?.accessToken) {
    throw new Error("Unauthorized");
  }

  const id = parseInt(this.params.id);

  let client: GraphQLClient | null = null;

  try {
    client = createClient(session.accessToken);

    const iterator = client.iterate<{ channel_events: MessageChannelEvent }>({
      query: `subscription Events($id: Int!) {
        channel_events(id: $id) {
          channelId
          eventType
          ... on MessageChannelEvent {
            message {
              id
              timestampMs
              text
              sender {
                sub
                name
              }
            }
          }
        }
      }`,
      variables: { id },
    });

    const abort = this.request.signal;
    abort.addEventListener("abort", () => {
      console.log('server abort');
      
      if (iterator.return)
        iterator.return();

        client?.dispose();
    });

    const joined: MessageChannelEvent = {
      channelId: id,
      eventType: "MessageChannelEvent",
      message: {
        id: 0,
        timestampMs: Date.now().toString(),
        text: "Joined",
        timestamp: new Date(),
        sender: {
          name: "System",
          sub: "system",
        },
      },
    };

    yield { channel_events: joined };

    while(true) {
      const ir = await iterator.next();
      
      if (ir.done || abort.aborted) {
        console.log('server done');
        break;
      }

      const event = ir.value;
      if (event.data?.channel_events.eventType === "MessageChannelEvent") {
        event.data.channel_events.message.timestamp = new Date(
          parseInt(event.data.channel_events.message.timestampMs)
        );
      }
      yield event.data;
    }
    console.log("iterator finished");
  } catch (e) {
    console.error("graphql error", e);
    throw e;
  } finally {
    client?.dispose();
    console.log("client disposed");
  }
});

export default component$(() => {
  const session = useAuthSession();
  const chat = useChatContext();
  const loc = useLocation();

  useTask$(async ({ track }) => {
    track(() => loc.params.id);
    const channel = await getChannel();
    chat.current = channel;
    console.log("current", channel.name);
  });

  /*useVisibleTask$(({ cleanup }) => {
    const timeout = setInterval(() => {
      getChannel().then((channel) => {
        chat.current = channel;
      });
    }, 200);

    cleanup(() => clearInterval(timeout));
  });*/

  useVisibleTask$(async ({ cleanup, track }) => {
    track(() => loc.params.id);
    console.log("using channel", loc.params.id);
    const abort = new AbortController();
    const iterator = await streamMessages(abort.signal);
    cleanup(()=> {
      console.log('abort');
      iterator.return();
      abort.abort();
    });    

    const update = setInterval(async () => {
      const ir = await iterator.next();

      if (ir.done) {
        console.log('done');
        return;
      }

      console.log("ir", ir.value);
      const e = ir.value?.channel_events;
      if (e?.eventType === "MessageChannelEvent") {
        console.log("pushing message", e.message);
        chat.current?.messages.push(e.message);
        console.log(chat.current?.messages.length);
      }
    }, 100);

    cleanup(() => clearInterval(update));
  });

  const text = useSignal("");
  const handleAddMessage = $(async () => {
    await addMessageServer(text.value);
    text.value = "";
  });

  return (
    <div>
      <div class="p-6 h-auto">
        <p class="text-lg">{chat.current?.name}</p>
      </div>
      <div class="p-6 pt-0 overflow-y-auto w-6/12" style={{ height: "85vh" }}>
        {chat.current?.messages.map((message) => (
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
        <div>
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
                bind:value={text}
              />
            </div>
            <button
              class="btn btn-outline btn-secondary rounded-sm w-16 h-12 col-span-1"
              onClick$={handleAddMessage}
              disabled={text.value.length === 0}
            >
              Send
            </button>
          </div>
        </div>
      </div>
    </div>
  );
});
