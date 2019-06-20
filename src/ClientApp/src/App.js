import React, { Component } from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import { Home } from './components/Home';
import { Channel } from './components/Channel';

export default class App extends Component {
  displayName = App.name

  render() {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/channels/:id' component={Channel} />        
      </Layout>
    );
  }
}
