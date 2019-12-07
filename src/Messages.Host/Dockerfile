FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
COPY [".config", ""]
RUN dotnet tool restore

WORKDIR /src
COPY ["src/Messages.Host/Messages.Host.csproj", "src/Messages.Host/"]

RUN dotnet restore "src/Messages.Host/Messages.Host.csproj"
COPY . .
WORKDIR "/src/src/Messages.Host"
RUN dotnet build "Messages.Host.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Messages.Host.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "tanka.graphql.samples.messages.host.dll"]