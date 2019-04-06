# Tanka GraphQL sample

Example of using tanka-graphql with Apollo


## Projects

* Host - this acts as an gateway to Channels and Messages services
* Channels.Host - provides channels graphql service
* Messages.Host - provides messages graphql service

Host projects uses introspection to build remote executable schemas 
from Channels and Messages services.


## Requirements

- nodeJS
- latest VS 2017 or 2019
- dotnet SDK 2.2


## Startup

Select multiple projects in the debug options of the solution
and make sure Host is the last one to launch.
