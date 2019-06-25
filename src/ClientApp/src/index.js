import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter } from "react-router-dom";
import { ApolloProvider } from "react-apollo";

import App from "./App";
import auth from "./auth";
import clientFactory from "./client";

const protocols = [
  "sr",
  "ws"];

const protocol = protocols[random(0, 1)];

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href");
const rootElement = document.getElementById("root");

const handleAuthentication = location => {
  if (/access_token|id_token|error/.test(location.hash)) {
    return auth.handleAuthentication();
  }
  return Promise.reject();
};

if (/callback/.test(window.location.pathname)) {
  handleAuthentication(window.location).then(
    () => (window.location.href = baseUrl),
    reason => console.log("Callback error", reason)
  );
} else if (auth.isLoggedIn()) {
  auth.renewSession().then(
    () => {
      ReactDOM.render(
        <ApolloProvider client={clientFactory(protocol)}>
          <BrowserRouter basename={baseUrl}>
            <App />
          </BrowserRouter>
        </ApolloProvider>,
        rootElement
      );
    },
    () => auth.login()
  );
} else {
  auth.login();
}

function random(min, max) {
  return Math.floor(Math.random() * (max - min + 1) + min);
}
