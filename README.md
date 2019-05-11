# Tanka GraphQL sample

Example of using tanka-graphql with Apollo

Live at: https://tanka-chat.azurewebsites.net


## Projects

* Host - this acts as an gateway to Channels and Messages services
* Host/ClientApp - SPA frontend using React and Apollo
* Host.WebSockets - same as Host but with WebSockets server
* Host.WebSockets/ClientApp - same as Host/ClientApp but uses apollo-link-ws
* Channels.Host - provides channels graphql service
* Messages.Host - provides messages graphql service

Host project uses introspection to build remote executable schemas 
from Channels and Messages services.

Host project also hosts the frontend SPA application.


## Requirements

- nodeJS
- latest VS 2017 or 2019 (or just use command prompt and VS Code)
- dotnet SDK 2.2
- Auth0 tenant with SPA and API applications created


## Solution

Sample uses Auth0 to provide GitHub login for the application. The default values in the repo
might not work for you and it's recommended you configure your own tenant in Auth0 and modify
settings to match.


### Frontend

Frontend uses Auth0 for authentication. Create SPA application in Auth0 and modify .env file at the
root of `src\Host\ClientApp` directory with the settings of your application.

Example:
```bash
REACT_APP_CLIENT_ID=<yourid>
REACT_APP_DOMAIN=<yourdomain>
REACT_APP_REDIRECT_URI='https://localhost:5001/callback'
REACT_APP_RESPONSE_TYPE='token id_token'
REACT_APP_SCOPE='openid'
```


#### Host/ClientApp
Url: https://localhost:5001

React frontend application hosted by the Host project using ASP.NET Core SPA services. This could
be separate application but hosting it inside the ASP.NET Core simplifies the startup and deployment.


#### Host.WebSockets/ClientApp
Url: https://localhost:5002

Same as above but uses apollo-link-ws instead of the SignalR based link


### Backend

Select multiple projects in the debug options of the solution
and make sure Host is the last one to launch.

Backend uses Auth0 for JWT validation and authorization. Create API in Auth0 and set your settings
in appsettings.json files in the web projects in the solution.

```json
"JWT": {
	"Authority": "<authority",
	"Audience": "<audience>"
}
```


#### Host

Host project uses the introspection query to build a single executable schema from the GraphQL
endpoints provided by Messages.Host and Channels.Host. This allows splitting the application 
into multiple smaller services while still providing single access point for the clients. Provides
SignalR based GraphQL server for clients using tanka-graphql-server-link.


#### Host.WebSockets

Same as above but client communication is handled using Tanka websocket server using apollo-link-ws.


#### Messages.Host

Messages host uses Auth0 userinfo endpoint to fetch additional claims about the user. It also
add the current user into the arguments of the resolver so resolver can access the `ClaimsPrincipal`.

Messages host also provides a streaming subscription support for chat message events.


#### Channels.Host

Channels host provides functions for getting list of channels.