# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first (better layer caching): copy solution + project files only.
COPY TrojesDeMaranon.sln ./
COPY src/TrojesDeMaranon.Api/TrojesDeMaranon.Api.csproj src/TrojesDeMaranon.Api/
COPY src/TrojesDeMaranon.Application/TrojesDeMaranon.Application.csproj src/TrojesDeMaranon.Application/
COPY src/TrojesDeMaranon.Domain/TrojesDeMaranon.Domain.csproj src/TrojesDeMaranon.Domain/
COPY src/TrojesDeMaranon.Infrastructure/TrojesDeMaranon.Infrastructure.csproj src/TrojesDeMaranon.Infrastructure/
COPY src/TrojesDeMaranon.Persistence/TrojesDeMaranon.Persistence.csproj src/TrojesDeMaranon.Persistence/
RUN dotnet restore TrojesDeMaranon.sln

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/TrojesDeMaranon.Api/TrojesDeMaranon.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production
# Cloud Run injects $PORT (default 8080). Bind Kestrel to it; default to 8080 locally.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Shell form so ${PORT} expands at runtime when Cloud Run overrides it.
CMD ASPNETCORE_URLS="http://+:${PORT:-8080}" dotnet TrojesDeMaranon.Api.dll
