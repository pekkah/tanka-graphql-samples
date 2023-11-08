import { createResource } from "solid-js";

const [data] = createResource(async () => {
  const response = await fetch("/session", {
    redirect: "error",
  });
  return await response.json();
});

export default function Home() {
  return (
    <>
      {data.error && <div>{data.error}</div>}
      {data.loading ? (
        <div>Loading...</div>
      ) : (
        <div>
          {data().isAuthenticated ? (
            <div>
              Hello! {data().name} <a href="/signout">Logout</a>
            </div>
          ) : (
            <div>
              Please <a href="/signin">Login</a>
            </div>
          )}
        </div>
      )}
    </>
  );
}
