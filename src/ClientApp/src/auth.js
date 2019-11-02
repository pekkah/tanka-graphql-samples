import auth0 from "auth0-js";

const config = window.CONFIG || {};
console.debug("CONFIG", config);

class Auth {
  accessToken = null;
  idToken = null;
  expiresAt;
  userProfile;
  tokenRenewalTimeout;
  auth0 = new auth0.WebAuth({
    domain: config.domain || process.env.REACT_APP_DOMAIN,
    audience: config.audience ||process.env.REACT_APP_AUDIENCE,
    clientID: config.clientID ||process.env.REACT_APP_CLIENT_ID,
    redirectUri: config.redirectUri ||process.env.REACT_APP_REDIRECT_URI,
    responseType: config.responseType || process.env.REACT_APP_RESPONSE_TYPE,
    scope: config.scope || process.env.REACT_APP_SCOPE
  });

  constructor() {
    this.login = this.login.bind(this);
    this.logout = this.logout.bind(this);
    this.handleAuthentication = this.handleAuthentication.bind(this);
    this.isAuthenticated = this.isAuthenticated.bind(this);
    this.getAccessToken = this.getAccessToken.bind(this);
    this.getIdToken = this.getIdToken.bind(this);
    this.renewSession = this.renewSession.bind(this);
    this.getProfile = this.getProfile.bind(this);
    this.getExpiryDate = this.getExpiryDate.bind(this);
  }

  login() {
    this.auth0.authorize();
  }

  handleAuthentication() {
    return new Promise((resolve, reject) =>
      this.auth0.parseHash((err, authResult) => {
        if (authResult && authResult.accessToken && authResult.idToken) {
          this.setSession(authResult);
          resolve();
        } else if (err) {
          console.log(err);
          reject(err);
        }
      })
    );
  }

  getAccessToken() {
    return this.accessToken;
  }

  getIdToken() {
    return this.idToken;
  }

  setSession(authResult) {
    // Set isLoggedIn flag in localStorage
    localStorage.setItem("isLoggedIn", "true");

    // Set the time that the access token will expire at
    let expiresAt = authResult.expiresIn * 1000 + new Date().getTime();
    this.accessToken = authResult.accessToken;
    this.idToken = authResult.idToken;
    this.expiresAt = expiresAt;

    // schedule a token renewal
    this.scheduleRenewal();
  }

  renewSession() {
    return new Promise((resolve, reject) =>
      this.auth0.checkSession({}, (err, authResult) => {
        if (authResult && authResult.accessToken && authResult.idToken) {
          this.setSession(authResult);
          resolve();
        } else if (err) {
          this.logout();
          console.log(err);
          reject(err);
        }
      })
    );
  }

  getProfile() {
    return new Promise((resolve, reject) =>
      this.auth0.client.userInfo(this.accessToken, (err, profile) => {
        if (profile) {
          this.userProfile = profile;
          resolve(this.userProfile);
        } else {
          reject(err);
        }
      })
    );
  }

  logout() {
    // Remove tokens and expiry time
    this.accessToken = null;
    this.idToken = null;
    this.expiresAt = 0;

    // Remove user profile
    this.userProfile = null;

    // Clear token renewal
    clearTimeout(this.tokenRenewalTimeout);

    // Remove isLoggedIn flag from localStorage
    localStorage.removeItem("isLoggedIn");

    this.auth0.logout({
      clientID: process.env.REACT_APP_CLIENT_ID,
      returnTo: window.location.origin
    });
  }

  isAuthenticated() {
    // Check whether the current time is past the
    // access token's expiry time
    let expiresAt = this.expiresAt;
    return new Date().getTime() < expiresAt;
  }

  isLoggedIn() {
    var value = localStorage.getItem("isLoggedIn");

    if (value == null) return false;

    return value;
  }

  scheduleRenewal() {
    let expiresAt = this.expiresAt;
    const timeout = expiresAt - Date.now();
    if (timeout > 0) {
      this.tokenRenewalTimeout = setTimeout(() => {
        this.renewSession();
      }, timeout);
    }
  }

  getExpiryDate() {
    return JSON.stringify(new Date(this.expiresAt));
  }
}

export default new Auth();
