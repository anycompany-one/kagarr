# Frontend build stage
FROM node:20-alpine AS frontend
WORKDIR /frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build

# Backend build stage
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
COPY src/Kagarr.Test.Common/Kagarr.Test.Common.csproj src/Kagarr.Test.Common/
COPY src/Kagarr.Core.Test/Kagarr.Core.Test.csproj src/Kagarr.Core.Test/
COPY src/Directory.Build.props src/
COPY src/Kagarr.sln src/
COPY global.json .
COPY stylecop.json .

RUN dotnet restore src/Kagarr.sln

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/Kagarr.Console/Kagarr.Console.csproj -c Release -o /app --no-restore

# Copy built frontend into the app output
COPY --from=frontend /src/Kagarr.Host/UI /app/UI

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV PUID=1000 \
    PGID=1000 \
    KAGARR_DATA=/config

EXPOSE 6767

VOLUME ["/config", "/games", "/downloads"]

COPY --from=build /app .

ENTRYPOINT ["dotnet", "Kagarr.dll", "--data=/config"]
