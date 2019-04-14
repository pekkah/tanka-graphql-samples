import React, { Component } from "react";
import gql from "graphql-tag";
import { Query } from "react-apollo";
import { withStyles } from "@material-ui/core/styles";
import Avatar from "@material-ui/core/Avatar";
import List from "@material-ui/core/List"
import ListItem from "@material-ui/core/ListItem"
import ListItemText from '@material-ui/core/ListItemText';
import ListItemAvatar from '@material-ui/core/ListItemAvatar';

const GET_MESSAGES = gql`
  query Messages($channelId: Int!) {
    messages(channelId: $channelId) {
      id
      channelId
      content
      timestamp
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
                  if (prev.messages !== undefined)
                    messages = prev.messages;

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

const channelMessagesStyles = {
  card: {
    minHeight: 75,
    marginBottom: 16
  }
};

const MessageList = withStyles(channelMessagesStyles)(
  class extends Component {
    componentDidMount() {
      this.props.subscribeToNewMessages();
    }

    render() {
      const {
        loading,
        data: { messages }
      } = this.props;

      if (loading)
        return (<span>Loading..</span>);

      return (
        <div>
          <List>
            {messages &&
              messages.map(message => (
                <ListItem key={message.id}>
                  <ListItemAvatar>
                    <Avatar>XX</Avatar>
                  </ListItemAvatar>
                  <ListItemText secondary={message.timestamp} primary={message.content}/>
                </ListItem>
              ))}
          </List>
        </div>
      );
    }
  }
);
