import { Link } from "react-router-dom";
import { useChannels } from "../data/useChannels";

export function ChannelsList() {
  const { fetching, data } = useChannels();

  if (fetching) {
    return <div>Loading...</div>;
  }

  const channels = data.channels;
  return (
    <>
      <li>
        <h3 class="menu-title">Channels</h3>
      </li>
      {channels.map((channel) => (
        <li key={channel.id}>
          <Link to={`/channels/${channel.id}`}>{channel.name}</Link>
        </li>
      ))}
    </>
  );
}
