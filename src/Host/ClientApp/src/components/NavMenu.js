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

import "./NavMenu.css";

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
              <div>
                <Navbar
                  className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3"
                  light
                >
                  <Container>
                    <NavbarBrand tag={Link} to="/">
                      Chat
                    </NavbarBrand>
                    <NavbarToggler
                      onClick={this.toggleNavbar}
                      className="mr-2"
                    />
                    <Collapse
                      className="d-sm-inline-flex flex-sm-row-reverse"
                      isOpen={!this.state.collapsed}
                      navbar
                    >
                      <ul className="navbar-nav flex-grow" />
                    </Collapse>
                  </Container>
                </Navbar>
                <Nav pills>
                  {data.channels.map(channel => (
                    <NavItem key={channel.id}>
                      <NavLink tag={Link} to={`/channels/${channel.id}`}>
                        {channel.name}
                      </NavLink>
                    </NavItem>
                  ))}
                </Nav>
              </div>
            );
          }}
        </Query>
      </header>
    );
  }
}
