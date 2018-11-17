import React, { Component } from "react";
import gql from "graphql-tag";
import { Query, Mutation } from "react-apollo";

import { withStyles } from '@material-ui/core/styles';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import TextField from '@material-ui/core/TextField';

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
}

const GET_MESSAGES = gql`
  query Messages($id: Int!) {
      messages(channelId: $id) {
        id
        content
      }
  }
`;

const channelMessagesStyles = {
  card: {
    minHeight: 75,
    marginBottom: 16
  }
};


const ChannelMessages = withStyles(channelMessagesStyles)(({ id, classes }) => {
  return (
    <Query query={GET_MESSAGES} variables={{ id }}>
      {({ loading, error, data }) => {
        if (loading) return "Loading..";
        if (error) return `Error: ${error}`;

        const messages = data.messages;

        return (
          <div>
            {messages.map(message => (
              <Card className={classes.card} key={message.id}>
                <CardContent>
                  <Typography component="p">
                    {message.content}
                  </Typography>
                </CardContent>
              </Card>
            ))}
          </div>
        );
      }}
    </Query>
  );
});

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
    this.state = {message: ""};
  }

  onMessageChanged(e) {
    this.setState({message: e.target.value});
  }

  render() {
    const { id } = this.props;
    return (
      <Mutation
        mutation={POST_MESSAGE}
        refetchQueries={result => {
          return [
            {
              query: GET_MESSAGES,
              variables: { id }
            }
          ];
        }}
      >
        {(postMessage, { data }) => (
          <div>
            <form
              onSubmit={e => {
                e.preventDefault();
                const options = { variables: { channelId: id, message: { content: this.state.message } } };
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