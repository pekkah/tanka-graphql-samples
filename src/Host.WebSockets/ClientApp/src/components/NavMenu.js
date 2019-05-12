import React, { Component } from "react";
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import InboxIcon from '@material-ui/icons/MoveToInbox';

import { Link } from "react-router-dom";
import gql from "graphql-tag";
import { Query } from "react-apollo";


const GET_CHANNELS = gql`
  {
    channels {
      id
      name
    }
  }
`;

export class NavMenu extends Component {
  displayName = NavMenu.name;

  constructor(props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar() {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render() {
    return (
      <header>
        <Query query={GET_CHANNELS}>
          {({ loading, error, data }) => {
            if (loading) return "Loading...";
            if (error) return `Error! ${error.message}`;

            return (
              <>
                <List>
                  {data.channels.map(channel => (
                    <ListItemLink to={`/channels/${channel.id}`} primary={channel.name} icon={<InboxIcon />} key={channel.id}/>
                  ))}
                </List>
              </>
            );
          }}
        </Query>
      </header>
    );
  }
}

class ListItemLink extends React.Component {
  renderLink = itemProps => <Link to={this.props.to} {...itemProps} />;

  render() {
    const { icon, primary, secondary } = this.props;
    return (
      <li>
        <ListItem button component={this.renderLink}>
          {icon && <ListItemIcon>{icon}</ListItemIcon>}
          <ListItemText inset primary={primary} secondary={secondary} />
        </ListItem>
      </li>
    );
  }
}
