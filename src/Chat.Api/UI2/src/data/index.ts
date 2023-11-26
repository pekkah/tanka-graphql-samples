import {
  Client,
  cacheExchange,
  fetchExchange,
} from "@urql/core";

const client = new Client({
  url: "/graphql",
  exchanges: [cacheExchange, fetchExchange],
});

export { client };
