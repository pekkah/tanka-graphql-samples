import { createResource } from "solid-js";

const [session, { mutate: _, refetch: refreshSession }] = createResource<Session>(getSession);

async function getSession(): Promise<Session> {
  var response = await fetch("/session", {
    method: "GET",
    redirect: "manual",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (response.status === 200) {
    const session = await response.json() as SessionBase;

    if (session.isAuthenticated)
        return session as AuthenticatedSession;

    return undefined;
  }

  return undefined;
}

export {
  type Session,
  type AuthenticatedSession,
  refreshSession,
  session,
};

type SessionBase = {
    isAuthenticated: boolean;
}

type AuthenticatedSession = {
  isAuthenticated: true;
  login: string;
  name: string;
  id: string;
  avatarUrl: string;
};

type Session = AuthenticatedSession | undefined;
