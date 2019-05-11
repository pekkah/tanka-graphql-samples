import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter } from "react-router-dom";
import { ApolloProvider } from "react-apollo";

import App from "./App";
import auth from "./auth";
import clientFactory from "./client";

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href");
const rootElement = document.getElementById("root");

const handleAuthentication = location => {
  if (/access_token|id_token|error/.test(location.hash)) {
    return auth.handleAuthentication();
  }
  return Promise.reject();
};

if (window.location.pathname === "/callback") {
  handleAuthentication(window.location).then(
    () => window.location.href = "/",
    reason => console.log("Callback error", reason)
  );
} else if (auth.isLoggedIn()) {
  const client = clientFactory();
  auth.renewSession().then(() => {
    ReactDOM.render(
      <ApolloProvider client={client}>
        <BrowserRouter basename={baseUrl}>
          <App />
        </BrowserRouter>
      </ApolloProvider>,
      rootElement
    );
  });
} else {
  auth.login();
}
