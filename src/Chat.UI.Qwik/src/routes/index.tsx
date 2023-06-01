import { component$ } from "@builder.io/qwik";
import type { DocumentHead } from "@builder.io/qwik-city";
import { Form } from "@builder.io/qwik-city";
import { useAuthSession, useAuthSignin } from "./plugin@auth";
import { useAuthSignout } from "./plugin@auth";

export default component$(() => {
  const signIn = useAuthSignin();
  const signOut = useAuthSignout();
  const session = useAuthSession();

  return (
    <>
      {!session.value?.user ? (
        <Form action={signIn}>
          <input type="hidden" name="providerId" value="github" />
          <button class="btn btn-primary btn-sm">Sign In</button>
        </Form>
      ) : (
        <>
          <Form action={signOut}>
            <button class="btn btn-warning btn-sm">Sign Out</button>
          </Form>
          <div>Hi {session.value.user.name}</div>
        </>
      )}
    </>
  );
});

export const head: DocumentHead = {
  title: "Tanka Chat - Qwik",
  meta: [
    {
      name: "description",
      content: "Simple Tanka GraphQL chat app",
    },
  ],
};
