import { useParams } from "react-router-dom";
import { useChannelWithNewMessages } from "../../data/useChannels";
import { useAddMessage } from "../../data/useAddMessage";
import { signal } from "@preact/signals";
import { Message } from "../../generated/graphql";
import { Session, useSession } from "../../model";
import { useEffect, useRef } from "preact/hooks";
import { usePageTitle } from "../../model/page";

export default function Channel() {
  const { id } = useParams<{ id: string }>();
  const [addMessageResponse, addMessage] = useAddMessage();
  const newMessage = signal("");

  const session = useSession();
  const [
    {
      data,
      error,
      fetching,
    },
  ] = useChannelWithNewMessages(parseInt(id));

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  const { channel } = data;
  const messages = channel.messages || [];

  function handleNewMessageChange(e: Event) {
    const target = e.target as HTMLInputElement;
    newMessage.value = target.value;
  }

  function addMessageClick() {
    addMessage({ channelId: channel.id, text: newMessage.value });
    newMessage.value = "";
  }

  usePageTitle().value = channel.name;

  return (
    <>
      <div class="overflow-y-auto h-[85%] mb-2">
        <MessageList messages={messages} session={session.value} />
      </div>
      <div class="items-center flex bottom-0 h-[50px] m-4">
        <input
          type="text"
          value={newMessage}
          class="w-full px-4 py-2 border border-gray-300 rounded-l-md"
          placeholder="Type your message..."
          onInput={handleNewMessageChange}
          disabled={addMessageResponse.fetching}
        />
        <button
          class="px-4 py-2 bg-blue-500 text-white rounded-r-md"
          disabled={addMessageResponse.fetching || fetching}
          onClick={addMessageClick}
        >
          Send
        </button>
      </div>
    </>
  );
}

function MessageList({
  messages,
  session,
}: {
  messages: Partial<Message>[];
  session: Session;
}) {
  function isByCurrentUser(message: Partial<{ sender: { id: string } }>) {
    if (!session.isAuthenticated) return false;

    return message.sender.id === session.id;
  }

  const messagesDiv = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (messagesDiv.current) {
      messagesDiv.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [messages]);

  return (
    <>
      {messages.map((message) => (
        <div class="m-2" ref={messagesDiv}>
          <div
            key={message.id}
            class={`chat ${
              isByCurrentUser(message) ? "chat-end" : "chat-start"
            }`}
          >
            <div class="chat-image avatar">
              <div class="w-10 rounded-full">
                <img alt={message.sender.name} src={message.sender.avatarUrl} />
              </div>
            </div>
            <div class="chat-bubble">{message.text}</div>
          </div>
        </div>
      ))}
    </>
  );
}
