import {
  Client,
  DocumentInput,
  OperationContext,
  OperationResult,
  cacheExchange,
  fetchExchange,
} from "@urql/core";
import {
  Accessor,
  createResource,
} from "solid-js";

const client = new Client({
  url: "/graphql",
  exchanges: [cacheExchange, fetchExchange],
});

function createQuery<Data extends object = any, Variables extends object = any>(
  query: Accessor<DocumentInput<Data, Variables>>,
  variables: Accessor<Variables>,
  context?: Accessor<Partial<OperationContext>>
) {
  const [resource] = createResource<
    OperationResult<Data>,
    readonly [
      DocumentInput<Data, Variables>,
      Variables,
      Partial<OperationContext> | undefined
    ]
  >(
    () => [query(), variables?.(), context?.()] as const,
    async ([q, v, c]) => {
      const result = await client.query(q, v, c);
      return result;
    }
  );

  return resource;
}

export { client, createQuery };
