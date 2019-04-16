# Tanka GraphQL sample

Example of using tanka-graphql with Apollo

Live at: https://todo


## Projects

* Host - this acts as an gateway to Channels and Messages services
* Channels.Host - provides channels graphql service
* Messages.Host - provides messages graphql service

Host project uses introspection to build remote executable schemas 
from Channels and Messages services.

Host project also hosts the frontend SPA application.


## Requirements

- nodeJS
- latest VS 2017 or 2019 (or just use command prompt and VS Code)
- dotnet SDK 2.2


## Getting started

### Frontend

Frontend uses Auth0 for authentication. Create SPA application in Auth0 and create .env file at the
root of `src\Host\ClientApp` directory with the settings of your application.

Example:
```
REACT_APP_CLIENT_ID=<yourid>
REACT_APP_DOMAIN=<yourdomain>
REACT_APP_REDIRECT_URI='https://localhost:5001/callback'
REACT_APP_RESPONSE_TYPE='token id_token'
REACT_APP_SCOPE='openid'
```


### Backend

Select multiple projects in the debug options of the solution
and make sure Host is the last one to launch.
