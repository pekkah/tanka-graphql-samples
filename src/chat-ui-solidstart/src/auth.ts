import { GitHubUser } from "solid-start-oauth/src/types";
import { storage } from "~/session";

export async function signUp({id}: GitHubUser) {
    const session = await storage.getSession();
    session.set("id", id);
    return redirect("/account", {
      headers: { "Set-Cookie": await storage.commitSession(session) },
    });
  }
  
  export async function signIn({ id }: { id: number }, redirectTo?: string) {
    const session = await storage.getSession();
    session.set("id", id);
    return redirect(redirectTo || "/account", {
      headers: { "Set-Cookie": await storage.commitSession(session) },
    });
  }