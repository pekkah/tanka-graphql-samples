# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a sample chat application demonstrating the Tanka GraphQL library capabilities. It features a .NET 8 backend with ASP.NET Core and a Preact frontend, implementing real-time chat with GraphQL queries, mutations, and subscriptions using the `graphql-ws` protocol.

## Essential Commands

### Build and Run
```bash
# Production build and run
dotnet build -c Release
dotnet run -c Release --launch-profile ChatProduction --project ./src/Chat.Api/

# Development (with manual Vite)
cd src/Chat.Api/UI2 && npm install  # First time only
npm run dev                          # In UI2 directory, separate terminal
dotnet run --project ./src/Chat.Api/
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestClassName"
```

### Frontend Development
```bash
cd src/Chat.Api/UI2
npm run codegen    # Regenerate GraphQL types after schema changes
npm run build      # Production frontend build
npm run dev        # Vite dev server with HMR
```

### Code Formatting
```bash
dotnet format      # Format C# code before committing
```

## Architecture

### GraphQL Schema Generation
The backend uses Tanka GraphQL with source generators. GraphQL types are defined using C# attributes in `src/Chat.Api/Schema/`:
- `[ObjectType]` for GraphQL object types
- `[InputType]` for input types  
- `[Union]` for union types
- Schema changes automatically regenerate at compile time

### Frontend Type Safety
1. Backend writes schema to `UI2/graphql/Default.graphql` via `WriteSchemaFiles.cs`
2. Frontend runs `npm run codegen` to generate TypeScript types
3. Generated types in `UI2/src/generated/` provide full type safety

### Real-time Subscriptions
- Uses WebSocket transport with `graphql-ws` protocol
- Subscription resolvers in `Subscriptions.cs`
- Frontend uses URQL client with `subscriptionExchange`

### Database
- SQLite with Entity Framework Core code-first approach
- Context in `ChatContext.cs`
- Migrations applied automatically on startup

## Development Workflow

### Making GraphQL Schema Changes
1. Modify C# types in `src/Chat.Api/Schema/`
2. Build backend: `dotnet build`
3. Regenerate frontend types: `cd src/Chat.Api/UI2 && npm run codegen`
4. Update frontend queries/mutations in `UI2/src/data/`

### Authentication Setup
Create `src/Chat.Api/appsettings.Local.json`:
```json
{
  "Authentication": {
    "GitHub": {
      "ClientId": "your-github-oauth-app-id",
      "ClientSecret": "your-github-oauth-app-secret"
    }
  }
}
```

### Vite Integration
- For faster development, run Vite manually: Set `Vite:AutoRun` to `false` in config
- Backend proxies to Vite dev server at `http://localhost:5173` during development
- Production builds are served from `wwwroot/` after `npm run build`

## Key Files and Patterns

### Backend Structure
- `Program.cs`: Application entry point and DI configuration
- `Schema/RootTypes.cs`: GraphQL Query, Mutation, Subscription root types
- `Schema/TypeDefinitions.cs`: Core domain types (Channel, Message, User)
- `Startup/`: Extension methods for configuring services

### Frontend Structure
- `UI2/src/data/`: GraphQL operations and custom hooks
- `UI2/src/pages/`: File-based routing with dynamic segments
- `UI2/src/components/`: Reusable UI components
- Uses Tailwind CSS with DaisyUI component library

### Testing Approach
- xUnit for backend tests
- Test project mirrors main project structure
- No frontend unit tests currently implemented