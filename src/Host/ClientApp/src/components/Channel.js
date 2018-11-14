import React, { Component } from "react";
import {} from "reactstrap";
import { Link } from "react-router-dom";
import gql from "graphql-tag";
import { Query } from "react-apollo";

const GET_CHANNEL = gql`
  query Channel($id: ID!) {
    channel(id: $id) {
      id
      name
      members {
        id
        name
      }
    }
  }
`;

const GET_MESSAGES = gql`
  query Messages($id: ID!, $latest: Int!) {
    channel(id: $id) {
      id
      messages(latest: $latest) {
        content
      }
    }
  }
`;

export class Channel extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    const channelId = this.props.match.params.id;
    return (
      <Query query={GET_CHANNEL} variables={{ id: channelId }}>
        {({ loading, error, data }) => {
          if (loading) return "Loading..";
          if (error) return `Error: ${error}`;

          return (
            <div>
              <h3>{data.name}</h3>
              <ChannelMessages id={channelId} />
            </div>
          );
        }}
      </Query>
    );
  }
}

export class ChannelMessages extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    const channelId = this.props.id;
    const messagesVariables = {
      id: channelId,
      latest: 100
    };

    return (
      <div>
        <Query query={GET_MESSAGES} variables={messagesVariables}>
          {({ loading, error, data }) => {
            if (loading) return "Loading..";
            if (error) return `Error: ${error}`;

            const messages = data.channel.messages;

            return (
              <div>
                {messages.map(message => (
                  <p>{message.content}</p>
                ))}
              </div>
            );
          }}
        </Query>
      </div>
    );
  }
}
