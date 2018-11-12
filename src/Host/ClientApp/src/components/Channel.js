import React, { Component } from "react";
import {
  Collapse,
  Container,
  Navbar,
  NavbarBrand,
  NavbarToggler,
  NavItem,
  NavLink,
  Nav
} from "reactstrap";
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

export class Channel extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    const channelId = this.props.match.params.id;
    return (
      <Query query={GET_CHANNEL} variables={{ id: channelId }}>
        {({ loading, error, data }) => {
          if (loading) return 'Loading..';
          if (error) return `Error: ${error}`;

          return (
            <div>
              <h3>{data.name}</h3>
            </div>
          );
        }}
      </Query>
    );
  }
}
