import { useMutation } from "@urql/preact";
import { graphql } from "../generated";

const AddMessageMutation = graphql(`
  mutation AddMessage($command: ChannelCommand!) {
    execute(command: $command) {
      __typename
      ... on AddMessageResult {
        message {
          id
          text
          timestampMs
          sender {
            id
            name
          }
        }
      }
    }
  }
`);

export function useAddMessage() {
  const response = useMutation(AddMessageMutation);
  return response;
}
