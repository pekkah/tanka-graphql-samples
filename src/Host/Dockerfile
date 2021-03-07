FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Host/Host.csproj", "src/Host/"]

RUN dotnet restore "src/Host/Host.csproj"
COPY . .
WORKDIR "/src/src/Host"
RUN dotnet build "Host.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "tanka.graphql.samples.Host.dll"]