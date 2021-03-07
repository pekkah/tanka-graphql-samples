FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY [".config", ""]
RUN dotnet tool restore

WORKDIR /src
COPY ["src/Channels.Host/Channels.Host.csproj", "src/Channels.Host/"]

RUN dotnet restore "src/Channels.Host/Channels.Host.csproj"
COPY . .
WORKDIR "/src/src/Channels.Host"
RUN dotnet build "Channels.Host.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Channels.Host.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "tanka.graphql.samples.channels.host.dll"]