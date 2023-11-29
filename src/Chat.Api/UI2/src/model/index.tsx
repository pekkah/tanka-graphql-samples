import { Signal, signal } from "@preact/signals";
import { createContext } from "preact";
import { useEffect, useContext } from "preact/hooks";

async function getSession(): Promise<Session> {
  var response = await fetch("/session", {
    method: "GET",
    redirect: "manual",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (response.status === 200) {
    const session = (await response.json()) as Session;

    if (session.isAuthenticated) return session as AuthenticatedSession;

    return session as UnauthenticatedSession;
  }

  return { isAuthenticated: false };
}

const session = signal<Session>({ isAuthenticated: false });
const SessionContext = createContext<Signal<Session>>(session);

function SessionProvider({ children }) {
  useEffect(() => {
    getSession().then((s) => (session.value = s));

    const update = setInterval(() => {
      console.log("Updating session");
      getSession().then((s) => (session.value = s));
    }, 1000 * 60 * 5);

    return () => clearInterval(update);
  }, []);

  return (
    <SessionContext.Provider value={session}>
      {children}
    </SessionContext.Provider>
  );
}

function useSession() {
  const context = useContext(SessionContext);

  if (context === undefined) {
    throw new Error("useSession must be used within a SessionProvider");
  }

  return context;
}

type AuthenticatedSession = {
  isAuthenticated: true;
  login: string;
  name: string;
  id: string;
  avatarUrl: string;
};

type UnauthenticatedSession = {
  isAuthenticated: false;
};

type Session = AuthenticatedSession | UnauthenticatedSession;

export {
  type Session,
  type AuthenticatedSession,
  type UnauthenticatedSession,
  SessionProvider,
  useSession,
};
