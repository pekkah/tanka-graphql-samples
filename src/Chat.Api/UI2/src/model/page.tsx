import { Signal, signal } from "@preact/signals";

const pageTitle = signal("");

export function usePageTitle(): Signal<string> {
  return pageTitle;
}

export function PageTitle() {
  const title = usePageTitle();

  return <h2 class="btn btn-ghost text-xl">{title}</h2>;
}
