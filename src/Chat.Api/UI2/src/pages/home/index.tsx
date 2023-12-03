import { useSession } from "../../model";

export default function Home() {
  const session = useSession();
  return (
    <div class="hero bg-base-200 rounded-xl">
      <div class="hero-content text-center">
        <div class="max-w-md">
          <h1 class="text-5xl font-bold">Hello there</h1>
          <p class="py-6">
            Simple chat application using Tanka GraphQL as backend with Preact
            frontend.
          </p>
          {!session.value.isAuthenticated && (
            <a class="btn btn-primary" href="/signin">
              Login
            </a>
          )}
        </div>
      </div>
    </div>
  );
}
