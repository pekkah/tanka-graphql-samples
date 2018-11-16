import React, { Component } from "react";
import {} from "reactstrap";
import { Link } from "react-router-dom";
import gql from "graphql-tag";
import { Query, Mutation } from "react-apollo";

const GET_CHANNEL = gql`
  query Channel($id: Int!) {
    channel(channelId: $id) {
      id
      name
    }
  }
`;

export const Channel = ({match}) => {
  const channelId = match.params.id;

  return (
      <Query query={GET_CHANNEL} variables={{ id: channelId }}>
        {({ loading, error, data }) => {
          if (loading) return "Loading..";
          if (error) return `Error: ${error}`;

          return (
            <div>
              <h3>{data.name}</h3>
              <ChannelMessages id={channelId} />
              <PostMessage id={channelId} />
            </div>
          );
        }}
      </Query>
    );
}

const GET_MESSAGES = gql`
  query Messages($id: Int!) {
      messages(channelId: $id) {
        id
        content
      }
  }
`;

const ChannelMessages = ({ id }) => {
    return (
         <Query query={GET_MESSAGES} variables={{id}}>
          {({ loading, error, data }) => {
            if (loading) return "Loading..";
            if (error) return `Error: ${error}`;

            const messages = data.messages;

            return (
              <div>
                {messages.map(message => (
                        <p key={message.id}>{message.content}</p>
                ))}
              </div>
            );
          }}
        </Query>
    );
}

const POST_MESSAGE = gql`
    mutation PostMessage($channelId: Int!, $message: InputMessage) {
        postMessage(channelId: $channelId, message: $message) {
            id
            content
        }
    }
`;

const PostMessage = ({id})=> {
    let input;

    return (
        <Mutation
            mutation={POST_MESSAGE}
            refetchQueries={result => {
                return [
                    {
                        query: GET_MESSAGES,
                        variables: {id}
                    }
                ];
            }}
            >
            {(postMessage, { data }) => (
            <div>
              <form
                onSubmit={e => {
                    e.preventDefault();
                    const options = { variables: { channelId: id, message: { content: input.value } } };
                    postMessage(options);
                    input.value = "";
                }}
              >
                <input
                  ref={node => {
                      input = node;
                  }}
                />
                <button type="submit">Post</button>
              </form>
            </div>
            )}
      </Mutation>
    );
}