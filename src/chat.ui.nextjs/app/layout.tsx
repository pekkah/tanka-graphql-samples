import Link from "next/link";
import "./globals.css";
import { Inter } from "next/font/google";

const inter = Inter({ subsets: ["latin"] });

export const metadata = {
  title: "Tanka Chat",
  description: "Chat app built with Tanka GraphQL",
};

async function getChannels() {
  try {
    const res = await fetch("https://localhost:8000/graphql", {
      method: "POST",
      cache: "no-cache",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
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

    return await res.json();
  } catch (err) {
    console.error("Failed to query channels", err);
    throw err;
  }
}

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const channelsResponse = await getChannels();
  const channels = channelsResponse.data.channels ?? [];

  return (
    <html lang="en">
      <body className={inter.className}>
        <div className="flex min-h-screen">
          <div className="bg-slate-900 p-4 text-white basis-1/6">
            <h1 className="text-xl mb-4">Channels</h1>
            {channels.map((channel: { id: string; name: string }) => (
              <Link href={`/channels/${channel.id}`} key={channel.id}>
                <div className="p-2 bg-fuchsia-700 text-white rounded-lg mb-1 hover:bg-fuchsia-600 cursor-pointer">
                  # {channel.name}
                </div>
              </Link>
            ))}
          </div>
          <div className="flex basis-5/6 text-white bg-slate-800">
            {children}
          </div>
        </div>
      </body>
    </html>
  );
}
