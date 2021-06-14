import React, { useEffect } from "react";
import gql from "graphql-tag";
import { useQuery, useSubscription } from "@apollo/react-hooks";
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

const ChannelMessages = (props) => {
  const { id } = props;
  const { loading, error, data, subscribeToMore } = useQuery(GET_MESSAGES, {
    variables: {
      channelId: id
    }
  });

  if (error)
    return <p>{error}</p>;

  if (loading)
    return <p>Loading</p>;

  return (
    <MessageList
      {...data}
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
  )
}

export { ChannelMessages };

var channelMessagesStyles = theme => ({
  root: {
    ...theme.mixins.gutters(),
    paddingTop: theme.spacing(2),
    paddingBottom: theme.spacing(2)
  },
  avatar: {
    width: 48,
    height: 48,
    margin: 10
  },
});

const MessageList = withStyles(channelMessagesStyles)((props) => {

  useEffect(()=> {
    const unsubscribe = props.subscribeToNewMessages();
    return () => unsubscribe();
  }, [props.subscribeToNewMessages])

  const {
    messages,
    classes
  } = props;

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
});