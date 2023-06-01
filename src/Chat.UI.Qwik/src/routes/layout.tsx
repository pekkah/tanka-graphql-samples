import { component$, Slot } from '@builder.io/qwik';

export default component$(() => {
  return (
    <>
      <main class="m-4">
        <Slot />
      </main>
    </>
  );
});
