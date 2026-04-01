---
title: "Docker for .NET Developers"
category: "DevOps"
date: "November 1, 2025"
readTime: "10 min read"
excerpt: "Containerize your .NET applications with Docker. Learn about multi-stage builds, optimization, and Docker Compose."
tags: ["Docker", "Containers", ".NET", "DevOps"]
sidebar:
  - href: "#basic-dockerfile"
    text: "Basic Dockerfile"
  - href: "#optimized-multi-stage-build"
    text: "Multi-Stage Builds"
  - href: "#docker-compose"
    text: "Docker Compose"
  - href: "#health-checks"
    text: "Health Checks"
  - href: "#image-size-optimization"
    text: "Optimization"
  - href: "#best-practices"
    text: "Best Practices"
---

Docker enables consistent deployment across environments. This guide covers containerizing .NET applications effectively.

## Basic Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

## Optimized Multi-Stage Build

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY ["src/MyApp.Api/MyApp.Api.csproj", "src/MyApp.Api/"]
COPY ["src/MyApp.Core/MyApp.Core.csproj", "src/MyApp.Core/"]
COPY ["src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "src/MyApp.Infrastructure/"]

# Restore as a separate layer
RUN dotnet restore "src/MyApp.Api/MyApp.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/MyApp.Api"
RUN dotnet build -c Release -o /app/build --no-restore

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN adduser -D appuser
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
```

## Native AOT Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MyApp.csproj", "."]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish \
    /p:PublishAot=true \
    /p:StripSymbols=true

# Use distroless or scratch for minimal image
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./MyApp"]
```

## .dockerignore

```
**/.vs
**/.vscode
**/bin
**/obj
**/.git
**/.gitignore
**/node_modules
**/*.md
**/Dockerfile*
**/.dockerignore
**/docker-compose*
**/*.user
**/*.sln.docstates
```

## Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: src/MyApp.Api/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=db;Database=MyApp;User=sa;Password=YourStrong!Password
    depends_on:
      - db
      - redis
    networks:
      - myapp-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Password
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - myapp-network

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
    networks:
      - myapp-network

networks:
  myapp-network:
    driver: bridge

volumes:
  sqldata:
```

### Development Override

```yaml
# docker-compose.override.yml
version: '3.8'

services:
  api:
    build:
      target: build
    volumes:
      - .:/src
      - ~/.nuget/packages:/root/.nuget/packages:ro
    environment:
      - DOTNET_USE_POLLING_FILE_WATCHER=1
    command: dotnet watch run --project src/MyApp.Api
```

## Health Checks

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

## Environment Configuration

```yaml
# docker-compose.yml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Logging__LogLevel__Default=Warning
    env_file:
      - .env.production
```

```bash
# .env.production
ConnectionStrings__Default=Server=prod-db;Database=MyApp;...
Redis__ConnectionString=prod-redis:6379
JWT__Secret=your-production-secret
```

## Useful Commands

```bash
# Build image
docker build -t myapp:latest .

# Run container
docker run -d -p 5000:8080 --name myapp myapp:latest

# View logs
docker logs -f myapp

# Execute command in container
docker exec -it myapp /bin/bash

# Docker Compose commands
docker-compose up -d          # Start all services
docker-compose down           # Stop all services
docker-compose logs -f api    # Follow logs
docker-compose build --no-cache  # Rebuild

# Clean up
docker system prune -a        # Remove unused data
docker volume prune           # Remove unused volumes
```

## Image Size Optimization

- Use Alpine-based images when possible
- Use multi-stage builds
- Minimize layers by combining RUN commands
- Use .dockerignore to exclude unnecessary files
- Consider Native AOT for smallest images

```
# Image size comparison
mcr.microsoft.com/dotnet/aspnet:10.0         ~220MB
mcr.microsoft.com/dotnet/aspnet:10.0-alpine  ~110MB
Native AOT + alpine                          ~30-50MB
Native AOT + scratch                         ~15-30MB
```

## Best Practices

- Use specific image tags, not `latest`
- Run as non-root user
- Use multi-stage builds
- Order Dockerfile commands for optimal caching
- Use health checks
- Don't store secrets in images
- Scan images for vulnerabilities
