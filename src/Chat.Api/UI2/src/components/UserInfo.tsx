import { useSession } from "../model";

export function UserInfo() {
  const session = useSession();

  return (
    <div>
      {session.value.isAuthenticated ? (
        <div class="dropdown dropdown-end">
          <label tabIndex={0} class="btn btn-ghost btn-circle avatar">
            <div class="w-8 rounded-full ring ring-info ring-offset-base-100 ring-offset-2">
              <img alt={session.value.login} src={session.value.avatarUrl} />
            </div>
          </label>
          <ul
            tabIndex={0}
            class="menu menu-sm dropdown-content z-[1] mt-3 p-2 shadow bg-base-100 rounded-box w-52"
          >
            <li>
              <a href="/signout">Logout</a>
            </li>
          </ul>
        </div>
      ) : (
        <div class="btn btn-ghost btn-circle avatar ring ring-info ring-offset-2">
          <a href="/signin">Login</a>
        </div>
      )}
    </div>
  );
}
