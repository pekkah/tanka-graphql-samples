"use server";

import { revalidatePath } from "next/cache";

export async function postMessage(data: FormData) {
  const channelIdStr = data.get("channelId")?.toString();

  if (!channelIdStr) {
    throw new Error("No channelId provided");
  }

  const channelId = Number.parseInt(channelIdStr);
  const message = data.get("message");

  try {
    const response = await fetch("https://localhost:8000/graphql", {
      method: "POST",
      cache: "no-cache",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        query: `mutation AddMessage($channelId: Int! $message: String!) {
                      channel(id: $channelId) {
                        message: addMessage(text: $message) {
                          id
                          channelId
                          text
                          timestampMs
                        }
                      }
                    }`,
        variables: {
          channelId,
          message,
        },
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to add message: ${response.statusText}`);
    }

    revalidatePath(`/channels/${channelId}`);
    console.log("postMessage", response.json());
  } catch (err) {
    console.error("Failed to add message", err);
    throw err;
  }
}
