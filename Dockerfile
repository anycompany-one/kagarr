# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/Kagarr.Common/Kagarr.Common.csproj src/Kagarr.Common/
COPY src/Kagarr.Core/Kagarr.Core.csproj src/Kagarr.Core/
COPY src/Kagarr.Http/Kagarr.Http.csproj src/Kagarr.Http/
COPY src/Kagarr.SignalR/Kagarr.SignalR.csproj src/Kagarr.SignalR/
COPY src/Kagarr.Api.V1/Kagarr.Api.V1.csproj src/Kagarr.Api.V1/
COPY src/Kagarr.Host/Kagarr.Host.csproj src/Kagarr.Host/
COPY src/Kagarr.Console/Kagarr.Console.csproj src/Kagarr.Console/
COPY src/Directory.Build.props src/
COPY src/Kagarr.sln src/

RUN dotnet restore src/Kagarr.sln

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/Kagarr.Console/Kagarr.Console.csproj -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Follow linuxserver.io patterns for PUID/PGID
ENV PUID=1000 \
    PGID=1000 \
    KAGARR_DATA=/config

EXPOSE 6767

VOLUME ["/config", "/games", "/downloads"]

COPY --from=build /app .

ENTRYPOINT ["dotnet", "Kagarr.dll", "--data=/config"]
