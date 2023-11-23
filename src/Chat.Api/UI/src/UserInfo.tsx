import { Match, Switch } from "solid-js";
import { session } from "./model";

export function UserInfo() {
  return (
    <Switch>
      <Match when={session()}>
        {(s) => (
          <div>
            <div class="dropdown dropdown-end">
              <label tabIndex={0} class="btn btn-ghost btn-circle avatar">
                <div class="w-12 rounded-full ring ring-info ring-offset-base-100 ring-offset-2">
                  <img alt={s().login} src={s().avatarUrl} />
                </div>
              </label>
              <ul
                tabIndex={0}
                class="menu menu-sm dropdown-content mt-3 z-[100] p-2 shadow bg-base-100 rounded-box w-52"
                style={"overflow: visible;"}
              >
                <li>
                  <a href="/signout">Logout</a>
                </li>
              </ul>
            </div>
          </div>
        )}
      </Match>
      <Match when={session() == undefined}>
        <div class="btn btn-ghost btn-circle avatar ring ring-info ring-offset-2">
          <a href="/signin" class="">Login</a>
        </div>
      </Match>
    </Switch>
  );
}
