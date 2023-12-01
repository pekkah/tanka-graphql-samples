import { UserInfo } from "./components/UserInfo";
import { ChannelsList } from "./components/ChannelList";
import { Link, Outlet } from "react-router-dom";
import { PageTitle } from "./model/page";

export default function Layout() {
  return (
    <div class="drawer lg:drawer-open max-h-screen p-0 m-0 w-full">
      <input id="my-drawer-2" type="checkbox" class="drawer-toggle" />
      <div class="drawer-content h-screen flex flex-col items-start w-full max-h-screen">
        <div class="navbar bg-base-100 flex-none">
          <div class="flex-none">
            <label for="my-drawer-2" class="btn drawer-button lg:hidden">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                class="inline-block w-5 h-5 stroke-current"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 6h16M4 12h16M4 18h16"
                ></path>
              </svg>
            </label>
          </div>
          <div class="flex-1">
            <PageTitle />
          </div>
          <div class="flex-none">
            <UserInfo />
          </div>
        </div>
        <div class="flex-1 w-full overflow-auto">
          <Outlet />
        </div>
      </div>
      <div class="drawer-side">
        <label
          for="my-drawer-2"
          aria-label="close sidebar"
          class="drawer-overlay"
        ></label>
        <div class="bg-base-200 min-h-full">
          <a class="btn btn-ghost text-xl p-4 text-primary" href="/">
            TANKA CHAT
          </a>
          <ul class="menu p-4 w-80 text-base-content">
            <ChannelsList />
          </ul>
        </div>
      </div>
    </div>
  );
}
