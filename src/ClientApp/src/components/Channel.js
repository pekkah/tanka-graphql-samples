import React, { useState } from "react";
import gql from "graphql-tag";
import { useQuery, useMutation } from '@apollo/react-hooks';
import { withStyles } from "@material-ui/core/styles";
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
  const { loading, error, data } = useQuery(GET_CHANNEL, {
    variables: {
      id: channelId
    }
  });

  if (loading)
    return <p>Loading</p>

  if (error)
    return <p>{error.message}</p>

  return (
    <div>
      <h3>{data.name}</h3>
      <ChannelMessages id={channelId} />
      <StyledPostMessage id={channelId} />
    </div>
  );
};

const POST_MESSAGE = gql`
  mutation PostMessage($channelId: Int!, $message: InputMessage!) {
    postMessage(channelId: $channelId, message: $message) {
      id
      content
    }
  }
`;


const PostMessage = (props) => {
  const [message, setMessage] = useState('');
  const [postMessage, { loading, error, data }] = useMutation(POST_MESSAGE);

  if (error)
    console.error("PostMessage", error);

  const onMessageChanged = (e) => {
    setMessage(e.target.value);
  }

  const { id, classes } = props;

  return (
    <div className={classes.root}>
      <form
        onSubmit={e => {
          e.preventDefault();
          const options = {
            variables: {
              channelId: id,
              message: { 
                content: message
              }
            }
          };
          postMessage(options);
          setMessage('');
        }}
      >

        <TextField
          label="Message"
          rowsMax="4"
          margin="normal"
          value={message}
          onChange={onMessageChanged}
          fullWidth={true}
        />
      </form>
    </div>
  );
}

const styles = theme => ({
  root: {
    margin: theme.spacing()
  }
});

const StyledPostMessage = withStyles(styles)(PostMessage);
