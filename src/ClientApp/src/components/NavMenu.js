import React from "react";
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import InboxIcon from '@material-ui/icons/MoveToInbox';

import { useHistory } from "react-router-dom";
import gql from "graphql-tag";
import { useQuery } from '@apollo/react-hooks';


const GET_CHANNELS = gql`
  {
    channels {
      id
      name
    }
  }
`;

const NavMenu = () => {
  const { loading, error, data } = useQuery(GET_CHANNELS);

  if (loading)
    return <p>Loading</p>

  if (error)
    return <p>{error.message}</p>

  const { channels } = data;

  return (
    <header>
      <List>
        {channels.map(channel => (
          <ListItemLink to={`/channels/${channel.id}`} primary={channel.name} icon={<InboxIcon />} key={channel.id} />
        ))}
      </List>
    </header>
  );
}

const ListItemLink = (props) => {
  const history = useHistory();
  const { icon, primary, secondary } = props;

  const handleListItemClick = (e, to) => {
    history.push(to);
  }

  return (
    <li>
      <ListItem button onClick={event => handleListItemClick(event, props.to)}>
        {icon && <ListItemIcon>{icon}</ListItemIcon>}
        <ListItemText inset primary={primary} secondary={secondary} />
      </ListItem>
    </li>
  );
}

export { NavMenu };
