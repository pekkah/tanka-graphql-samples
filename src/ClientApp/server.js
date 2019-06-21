const express = require('express');
const path = require('path');
const app = express();

// load config
require('dotenv').config()

const PORT = process.env.SERVER_PORT || 3000;

app.use(function (err, req, res, next) {
    console.error(err.stack)
    res.status(500).send('Something broke!')
  })

app.use('/static', express.static(path.join(__dirname, 'static')))

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
    console.debug("config-js", configJS);
    res.header('Content-Type', 'application/javascript');
    res.send(configJS);
});


app.get('/*', function(req, res) {
    console.log("req", req.url);
    res.sendFile(path.join(__dirname, 'index.html'));
  });

console.log(`Listening on ${PORT}`)
app.listen(PORT);