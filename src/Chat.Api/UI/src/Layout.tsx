import { Outlet } from "@solidjs/router";
import { UserInfo } from "./UserInfo";
import { ChannelsList } from "./ChannelList";

export default function Layout() {
  return (
    <div class="drawer lg:drawer-open">
      <input id="my-drawer-2" type="checkbox" class="drawer-toggle" />
      <div class="drawer-content flex flex-col items-start p-4">
        <div class="lg:hidden">
          <label
            for="my-drawer-2"
            class="btn btn-primary drawer-button lg:hidden"
          >
            Open channels
          </label>
        </div>
        <div class="fixed top-0 right-0 m-4">
          <UserInfo />
        </div>
        <div class="flex-1">
          <Outlet />
        </div>
      </div>
      <div class="drawer-side bg-base-200 p-2">
        <label
          for="my-drawer-2"
          aria-label="close sidebar"
          class="drawer-overlay"
        ></label>
        <div class="navbar">
          <div class="flex-1">
            <a class="btn btn-ghost text-xl" href="/">
              Tanka Chat
            </a>
          </div>
        </div>
        <ChannelsList />
      </div>
    </div>
  );
}


