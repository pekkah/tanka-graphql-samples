import React from 'react';
import gql from "graphql-tag";
import { Query } from "react-apollo";

const GET_HELLO = gql`
  {
    hello {
      message
    }
  }
`;

const Home = () => (
  <Query query={GET_HELLO}>
    {({ loading, error, data }) => {
      if (loading) return "Loading...";
      if (error) return `Error! ${error.message}`;

      return (
        <p>Hello, {data}</p>
      );
    }}
  </Query>
);

export { Home }