const express = require('express');
const fs = require("fs");
const path = require('path');
const app = express();

// load config
require('dotenv').config()

const PORT = process.env.SERVER_PORT || 3000;

app.use("*", function(req, res, next) {
  next();
});

app.get('/config.js', function(req, res) {
    var configJS = `window.CONFIG = {
        domain: "${process.env.REACT_APP_DOMAIN}",
        audience: "${process.env.REACT_APP_AUDIENCE}",
        clientID: "${process.env.REACT_APP_CLIENT_ID}",
        redirectUri: "${process.env.REACT_APP_REDIRECT_URI}",
        responseType: "${process.env.REACT_APP_RESPONSE_TYPE}",
        scope: "${process.env.REACT_APP_SCOPE}",
        gw: "${process.env.REACT_APP_HOST}"
    };`
    res.header('Content-Type', 'application/javascript');
    res.send(configJS);
});

app.use('/static', express.static(path.join(__dirname, 'static')))

app.get('*', function(req, res) {
  // this could be done on app start instead of every request
  fs.readFile("index.html", "utf8", function(err, data) {
    // dirty play to get the app running on subpath when homepage is not hardcoded 
    const modified = data
                    .replace(new RegExp('%BASE%', 'g'), process.env.REACT_APP_BASE)
                    .replace(new RegExp('/static/', 'g'), "static/");

    res.header("Content-Type", "text/html");
    res.send(modified);
    res.end();
  });    
});

app.use(function (err, req, res, next) {
  console.error(err.stack)
  res.status(500).send('Something broke!')
});

console.log(`Base path '${process.env.REACT_APP_BASE}'`);
console.log(`Listening on ${PORT}`);
app.listen(PORT);