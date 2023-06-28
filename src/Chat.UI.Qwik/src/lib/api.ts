import { JSONObject } from "@builder.io/qwik-city";
import { GraphQL } from "./graphql";
import { Channel } from "./types";

export async function fetchChannels(accessToken: string) {
  const response = await fetch("https://localhost:8000/graphql", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      query: `query Channels {
                channels {
                  id
                  name
                  description
                }
              }`,
    }),
  });

  if (!response.ok) {
    console.error(response.status, response.statusText);
    throw new Error(`Failed to fetch channels: ${response.statusText}`);
  }

  const json = await response.json();
  return (
    json as GraphQL<{
      channels: Channel[];
    }>
  ).data.channels;
}

export async function fetchChannel(id: number, accessToken: string) {
  const response = await fetch("https://localhost:8000/graphql", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      query: `query Channel($id: Int!) {
                channel(id: $id) {
                  id
                  name
                  description
                }
              }`,
      variables: { id },
    }),
  });

  if (!response.ok) {
    console.error(response.status, response.statusText);
    throw new Error(`Failed to fetch channels: ${response.statusText}`);
  }

  const json = await response.json();
  return (
    json as GraphQL<{
      channel: Channel;
    }>
  ).data.channel;
}

export async function fetchChannelAndMessages(id: number, accessToken: string) {
  const response = await fetch("https://localhost:8000/graphql", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      query: `query Channel($id: Int!) {
                  channel(id: $id) {
                    id
                    name
                    description
                    messages {
                      id
                      text
                      timestampMs
                      sender {
                        sub
                        name
                        avatarUrl
                      }
                    }
                  }
                }`,
      variables: { id },
    }),
  });

  if (!response.ok) {
    console.error('Failed to load channel and messages', response.status, response.statusText);
    throw new Error(`Failed to load channel and messages: ${response.statusText}`);
  }

  const json = (await response.json()) as GraphQL<{
    channel: Channel;
  }>;

  json.data.channel.messages = json.data.channel.messages.map((message) => {
    message.timestamp = new Date(parseInt(message.timestampMs));
    return message;
  });
  return json.data.channel;
}

export async function addMessage(data: JSONObject, accessToken: string, id: number) {
  const text = data.message;
  const response = await fetch("https://localhost:8000/graphql", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      query: `mutation AddMessage($id: Int! $text: String!) {
                  channel(id: $id) {
                    newMessage: addMessage(text: $text) {
                      id
                      text
                      timestampMs
                      sender {
                        sub
                        name
                        avatarUrl
                      }
                    }
                  }
                }`,
      variables: { id, text },
    }),
  });

  const json = (await response.json()) as GraphQL<{
    channel: {
      newMessage: {
        id: number;
        text: string;
        timestampMs: string;
        timestamp: Date;
        sender: {
          sub: string;
          name: string;
        };
      };
    };
  }>;

  json.data.channel.newMessage.timestamp = new Date(
    parseInt(json.data.channel.newMessage.timestampMs)
  );
  return json.data.channel;
}

