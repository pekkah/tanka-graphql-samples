import { component$ } from "@builder.io/qwik";
import { Link, useLocation } from "@builder.io/qwik-city";
import { useChatContext } from "./ChatContext";

export const Sidebar = component$(() => {
  const location = useLocation();
  const chat = useChatContext();

  return (
    <div class="menu mr-2 ml-2">
      <ul>
        {chat.channels.map((channel) => (
          <li key={channel.id}>
            <Link
              href={`/channels/${channel.id}`}
              class={[
                {
                  active: location.url.pathname === `/channels/${channel.id}/`,
                },
              ]}
            >
              <span class="text-fuchsia-400">#</span>
              {channel.name}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
});
