import { component$ } from "@builder.io/qwik";
import { Link } from "@builder.io/qwik-city";
import { Login } from "./Login";

export const Header = component$(() => {
  return (
    <div class="navbar">
      <div class="navbar">
        <div class="navbar-start">
          <Link href="/" class="btn btn-ghost normal-case text-xl">
            Tanka Chat
          </Link>
        </div>
        <div class="navbar-end">
          <Login />
        </div>
      </div>
    </div>
  );
});
