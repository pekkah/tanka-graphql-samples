import { type Session } from "@auth/core/types";
import { component$ } from "@builder.io/qwik";
import { type RequestHandler } from "@builder.io/qwik-city";
import { Login } from "~/components/Login";

export const onRequest: RequestHandler = (event) => {
  const session: Session | null = event.sharedMap.get("session");
  if (!session || new Date(session.expires) < new Date()) {
    console.log("redirecting to signin");
    event.redirect(302, `/api/auth/signin`);
  }
  else if (event.url.pathname === "/") {
    console.log("redirecting to channels");
    event.redirect(302, `/channels/`);
  }
};

export default component$(() => {
  return (
    <div class="p-6">
      <div>
        <Login />
      </div>
    </div>
  );
});
