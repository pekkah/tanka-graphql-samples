import "bootstrap/dist/css/bootstrap.css";
import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter } from "react-router-dom";
import { ApolloProvider } from "react-apollo";
import client from './client'

import App from "./App";
import registerServiceWorker from "./registerServiceWorker";

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href");
const rootElement = document.getElementById("root");

ReactDOM.render(
  <ApolloProvider client={client}>
    <BrowserRouter basename={baseUrl}>
      <App />
    </BrowserRouter>
  </ApolloProvider>,
  rootElement
);

registerServiceWorker();
