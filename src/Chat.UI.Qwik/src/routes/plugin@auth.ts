import { serverAuth$ } from "@builder.io/qwik-auth";
import GitHub from "@auth/core/providers/GitHub";
import type { Provider } from "@auth/core/providers";

declare module "@auth/core/types" {
  /**
   * Returned by `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
   */
  interface Session {
    accessToken: string;
    user: {
      name: string;
      firstName: string;
      lastName: string;
      sub: string;
    }
  }
}

export const { onRequest, useAuthSession, useAuthSignin, useAuthSignout } =
  serverAuth$(({ env }) => ({
    secret: env.get("AUTH_SECRET"),
    trustHost: true,
    debug: true,
    providers: [
      GitHub({
        clientId: env.get("GITHUB_ID")!,
        clientSecret: env.get("GITHUB_SECRET")!
      }),
    ] as Provider[],
    callbacks: {
      async jwt(params) {
        if (params.account?.access_token){
          params.token["accessToken"] = params.account.access_token;
        }

        return Promise.resolve(params.token);
      },
      async session({session, token }) {
        const names = token.name?.split(" ") ?? ["?", "?"];
        const firstName = names[0];
        const lastName = names[1];
        session.user.firstName = firstName;
        session.user.lastName = lastName;

        session.user.sub = token.sub ?? "??";
        session.accessToken = token["accessToken"] as string;
        return Promise.resolve(session);
      }
    }
  }));