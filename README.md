# Tanka Chat App built with Tanka GraphQL library

Simple chat application with channels and messages.

Use
s queries, mutations and subscriptions. Subscriptions use `graphql-ws` -protocol.

## Running the app

App is built with Asp.NET Core backend and Preact frontend. Backend acts as both API and host
for the frontend. 


### Prerequisites

- Application uses GitHub authentication and you must provide your own ClientId and ClientSecret
in the `appsettings.Local.json` -configuration file. 
- Application uses Node.js and npm to build the frontend. You must have Node.js installed on your machine.


### Running the app

```pwsh
dotnet build -c Release
dotnet run -c Release --launch-profile ChatProduction --project .\src\Chat.Api\
```

This will build the backend and frontend and run the app in "Production" -environment.


## Development

- Same as running the app
- Install frontend dependencies `npm install` in the `src\Chat.Api\UI2` -folder


### Editor

You can develop the app in two ways:
- Run Vite manually from command prompt: set the Vite:AutoRun false in your configuration and launch Vite manually 
`npm run dev` in the `src\Chat.Api\UI2` -folder.
- Let Visual Studoo start Vite automatically: set the Vite:AutoRun true in your configuration.

Running Vite manually is faster, but you need to remember to start it before running the app. 
Letting Visual Studio start Vite automatically is slower as Vite is started and stopped when you launch the app.



