import { component$, Slot } from "@builder.io/qwik";
import { Header } from "~/components/Header";
import { Sidebar } from "~/components/Sidebar";

export default component$(() => {
  return (
    <div>
      <div class="flex min-h-screen">
        <div class="w-96 bg-slate-900">
          <Header />
          <Sidebar />
        </div>
        <div class="w-full">
          <Slot />
        </div>
      </div>
    </div>
  );
});
