import { useMutation } from "@urql/preact";
import { graphql } from "../generated";

const AddMessageMutation = graphql(`
  mutation AddMessage($channelId: Int! $text: String!) {
    channel(id: $channelId) {
        addMessage(text: $text) {
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
`)

export function useAddMessage() {
    const response = useMutation(AddMessageMutation);
    return response;
}