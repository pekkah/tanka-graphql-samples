import { A, Outlet } from "@solidjs/router";

export default function Layout() {
  return (
    <div class="drawer lg:drawer-open">
      <input id="my-drawer-2" type="checkbox" class="drawer-toggle" />
      <div class="drawer-content flex flex-col items-center">
        <div class="lg:hidden">
          <label
            for="my-drawer-2"
            class="btn btn-primary drawer-button lg:hidden"
          >
            Open drawer
          </label>
        </div>
        <Outlet />
      </div>
      <div class="drawer-side">
        <label
          for="my-drawer-2"
          aria-label="close sidebar"
          class="drawer-overlay"
        ></label>
        <ul class="menu p-4 w-80 min-h-full bg-base-200 text-base-content">
          <li>
            <A href="/channels/1">General</A>
          </li>
          <li>
            <A href="/channels/2">Tanka</A>
          </li>
        </ul>
      </div>
    </div>
  );
}
