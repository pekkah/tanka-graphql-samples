import { component$ } from "@builder.io/qwik";
import type { DocumentHead } from "@builder.io/qwik-city";

export default component$(() => {
  return (
    <div class="p-6">
      <div>
        <p class="text-lg">Pick a channel from left</p>
      </div>
    </div>
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
