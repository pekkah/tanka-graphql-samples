import React, { Component } from "react";
import gql from "graphql-tag";
import { Query, Mutation, Subscription } from "react-apollo";
import TextField from "@material-ui/core/TextField";

import { ChannelMessages } from "./ChannelMessages";

const GET_CHANNEL = gql`
  query Channel($id: Int!) {
    channel(channelId: $id) {
      id
      name
    }
  }
`;

export const Channel = ({ match }) => {
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
};

const POST_MESSAGE = gql`
  mutation PostMessage($channelId: Int!, $message: InputMessage) {
    postMessage(channelId: $channelId, message: $message) {
      id
      content
    }
  }
`;

class PostMessage extends Component {
  constructor(props) {
    super(props);
    this.onMessageChanged = this.onMessageChanged.bind(this);
    this.state = { message: "" };
  }

  onMessageChanged(e) {
    this.setState({ message: e.target.value });
  }

  render() {
    const { id } = this.props;
    return (
      <Mutation mutation={POST_MESSAGE}>
        {(postMessage, { data }) => (
          <div>
            <form
              onSubmit={e => {
                e.preventDefault();
                const options = {
                  variables: {
                    channelId: id,
                    message: { content: this.state.message }
                  }
                };
                postMessage(options);
                this.setState({
                  message: ""
                });
              }}
            >
              <TextField
                label="Message"
                rowsMax="4"
                margin="normal"
                fullWidth={true}
                value={this.state.message}
                onChange={this.onMessageChanged}
                /*ref={node => {
                input = node;
            }}*/
              />
            </form>
          </div>
        )}
      </Mutation>
    );
  }
}
