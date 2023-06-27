import {
  useAuthSignin,
  useAuthSignout,
  useAuthSession,
} from "~/routes/plugin@auth";
import { component$, useComputed$ } from "@builder.io/qwik";
import { Form } from "@builder.io/qwik-city";

export const Login = component$(() => {
  const signIn = useAuthSignin();
  const signOut = useAuthSignout();
  const session = useAuthSession();
  const avatar = useComputed$(() => {
    const firstName = session.value?.user?.firstName;
    const lastName = session.value?.user?.lastName;

    if (!firstName || !lastName) return "??";

    return firstName[0] + lastName[0];
  });

  return (
    <>
      {session.value ? (
        <div class="dropdown">
          <label
            tabIndex={0}
            class="btn btn-ghost btn-circle avatar outline outline-1 outline-green-700"
          >
            <div class="avatar placeholder">
              <div class="bg-neutral-focus text-neutral-content rounded-full">
                <span>{avatar.value}</span>
              </div>
            </div>
          </label>
          <ul
            tabIndex={0}
            class="menu dropdown-content shadow bg-slate-800 border border-slate-600 w-32 mt-0"
          >
            <li>
              <a onClick$={() => signOut.submit({})} class="flex items-center">
                Logout
              </a>
            </li>
          </ul>
        </div>
      ) : (
        <Form action={signIn}>
          <input type="hidden" name="providerId" value="github" />
          {signIn.isRunning ? (
            <span class="loading loading-spinner text-secondary"></span>
          ) : (
            <button
              class="btn btn-primary btn-circle btn-md text-xs"
              disabled={signIn.isRunning}
            >
              Login
            </button>
          )}
        </Form>
      )}
    </>
  );
});
