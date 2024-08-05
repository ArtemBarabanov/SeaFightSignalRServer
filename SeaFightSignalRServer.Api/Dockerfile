#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SeaFightSignalRServer.Api/SeaFightSignalRServer.Api.csproj", "SeaFightSignalRServer.Api/"]
COPY ["SeaFightSignalRServer.Core/SeaFightSignalRServer.Core.csproj", "SeaFightSignalRServer.Core/"]
COPY ["SeaFightSignalRServer.Hub/SeaFightSignalRServer.Hubs.csproj", "SeaFightSignalRServer.Hubs/"]
RUN dotnet restore "./SeaFightSignalRServer.Api/SeaFightSignalRServer.Api.csproj"
COPY . .
WORKDIR "/src/SeaFightSignalRServer.Api"
RUN dotnet build "./SeaFightSignalRServer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SeaFightSignalRServer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeaFightSignalRServer.Api.dll"]