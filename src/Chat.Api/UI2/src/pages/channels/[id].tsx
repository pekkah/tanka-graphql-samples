import { useParams } from "react-router-dom";
import { usePageTitle } from "../../model/page";
import { useChannel } from "../../data/useChannels";
import { useAddMessage } from "../../data/useAddMessage";
import { signal } from "@preact/signals";

export default function Channel() {
  const { id } = useParams<{ id: string }>();

  const channelResponse = useChannel(parseInt(id));
  const [addMessageResponse, addMessage] = useAddMessage();
  const newMessage= signal("");

  if (channelResponse.fetching) {
    return <h2>Loading...</h2>;
  }

  const channel = channelResponse.data.channel;
  usePageTitle().value = channel.name;

  function handleNewMessageChange(e: Event) {
    const target = e.target as HTMLInputElement;
    newMessage.value = target.value;
  }

  function addMessageClick() {
    addMessage({ channelId: channel.id, text: newMessage.value });
    newMessage.value = "";
  }

  return (
    <div class="flex flex-col h-full">
      <div class="flex-grow overflow-y-auto">
        <p>messages</p>
      </div>
      <div class="h-[50px] flex items-center">
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
          disabled={addMessageResponse.fetching}
          onClick={addMessageClick}
        >
          Send
        </button>
      </div>
    </div>
  );
}
