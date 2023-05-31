import { createClient } from "./client";

const client = createClient({
  url: "https://localhost:8000/graphql",
  headers: () => {
    return {
      Authorization: "Bearer 123",
    };
  },
});

function query() {
  client.query();
}
