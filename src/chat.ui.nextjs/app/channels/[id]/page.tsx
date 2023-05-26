import { AddMessage } from "@/components/AddMessage";

// rome-ignore lint/suspicious/noExplicitAny: <explanation>
type PageProps<Params = any, SearchParams = any> = {
  params: Params;
  searchParams: SearchParams;
};

export async function getMessages(id: number) {
  try {
    const res = await fetch("https://localhost:8000/graphql", {
      method: "POST",
      cache: "no-cache",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        query: `query Messages($id: Int!) {
                    channel(id: $id) {
                      name
                      description
                      messages {
                        id
                        text
                        timestampMs
                      }
                    }
                  }`,
        variables: {
          id,
        },
      }),
    });

    return await res.json();
  } catch (err) {
    console.error("Failed to query messages", err);
    throw err;
  }
}

export default async function Channel({
  params: { id },
}: PageProps<{ id: number }>) {
  const messagesResponse = await getMessages(id);
  const channel = messagesResponse.data?.channel ?? {
    messages: [],
    name: "",
  };
  const messages = channel?.messages ?? [];
  console.log("messagesResponse", messagesResponse);
  return (
    <div className="relative w-full">
      <div className="p-4">
        <div className="mb-2 border-b border-slate-600 pb-1">
          <h1>{channel.name}</h1>
        </div>
        <div>
          {messages.map(
            (message: { id: number; text: string; timestampMs: string }) => {
              return (
                <div className="p-2 rounded-lg bg-gray-700 mb-1" key={message.id}>
                  <div className="font-bold">
                    <span className="text-gray-300">Username</span>{" "}
                    <span className="text-gray-400 text-sm">
                      {new Date(
                        Number.parseInt(message.timestampMs),
                      ).toLocaleString()}
                    </span>
                  </div>
                  <div className="mt-1 mb-1">{message.text}</div>
                </div>
              );
            },
          )}
        </div>
      </div>
      <div className="absolute bottom-0 p-4 w-full">
        <AddMessage channelId={id} />
      </div>
    </div>
  );
}
