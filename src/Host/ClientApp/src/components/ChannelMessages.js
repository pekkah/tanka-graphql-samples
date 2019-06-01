import React, { Component } from "react";
import gql from "graphql-tag";
import { Query } from "react-apollo";
import { withStyles } from "@material-ui/core/styles";
import List from "@material-ui/core/List";
import Avatar from '@material-ui/core/Avatar';
import Grid from '@material-ui/core/Grid';
import ListItem from "@material-ui/core/ListItem";
import Paper from "@material-ui/core/Paper";
import Typography from "@material-ui/core/Typography";

const GET_MESSAGES = gql`
  query Messages($channelId: Int!) {
    messages(channelId: $channelId) {
      id
      channelId
      content
      timestamp
      from
      profileUrl
    }
  }
`;

const MESSAGES_SUBSCRIPTION = gql`
  subscription MessageAdded($channelId: Int!) {
    messageAdded(channelId: $channelId) {
      id
      channelId
      content
      timestamp
      from
      profileUrl
    }
  }
`;

class ChannelMessages extends Component {
  constructor(props) {
    super(props);
    this.messages = [];
  }

  render() {
    const { id } = this.props;
    return (
      <Query query={GET_MESSAGES} variables={{ channelId: id }}>
        {({ subscribeToMore, ...result }) => (
          <MessageList
            {...result}
            subscribeToNewMessages={() =>
              subscribeToMore({
                document: MESSAGES_SUBSCRIPTION,
                variables: { channelId: id },
                updateQuery: (prev, { subscriptionData }) => {
                  if (!subscriptionData.data) return prev;
                  if (!subscriptionData.data.messageAdded) return prev;

                  const newMessage = subscriptionData.data.messageAdded;

                  let messages = [];
                  if (prev.messages !== undefined) messages = prev.messages;

                  return Object.assign({}, prev, {
                    messages: [...messages, newMessage]
                  });
                }
              })
            }
          />
        )}
      </Query>
    );
  }
}

export { ChannelMessages };

var channelMessagesStyles = theme => ({
  root: {
    ...theme.mixins.gutters(),
    paddingTop: theme.spacing.unit * 2,
    paddingBottom: theme.spacing.unit * 2
    },
  avatar: {
    width: 48,
    height: 48,
    margin:10
    },
});

const MessageList = withStyles(channelMessagesStyles)(
  class extends Component {
    componentDidMount() {
      this.props.subscribeToNewMessages();
    }

    render() {
      const {
        loading,
        data: { messages },
        classes
      } = this.props;

      if (loading) return <span>Loading..</span>;

      return (
        <div>
          <List>
            {messages &&
              messages.map(message => (
                  <ListItem key={message.id}>
                      <Grid container>
                          <Avatar className={classes.avatar} src={message.profileUrl} />
                          <Paper className={classes.root} elevation={1}>
                            <Typography variant="caption" component="h6">
                              {message.from} - {message.timestamp}
                            </Typography>
                            <Typography component="p">{message.content}</Typography>
                          </Paper>
                      </Grid>
                  </ListItem>
              ))}
          </List>
        </div>
      );
    }
  }
);
