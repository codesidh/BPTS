# Use the official .NET 8.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
# HTTPS port removed for Docker deployment

# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files first (for better Docker layer caching)
COPY ["src/WorkIntakeSystem.API/WorkIntakeSystem.API.csproj", "src/WorkIntakeSystem.API/"]
COPY ["src/WorkIntakeSystem.Core/WorkIntakeSystem.Core.csproj", "src/WorkIntakeSystem.Core/"]
COPY ["src/WorkIntakeSystem.Infrastructure/WorkIntakeSystem.Infrastructure.csproj", "src/WorkIntakeSystem.Infrastructure/"]

# Restore dependencies (this layer will be cached if project files don't change)
RUN dotnet restore "src/WorkIntakeSystem.API/WorkIntakeSystem.API.csproj"

# Copy only the necessary source code for API
COPY ["src/WorkIntakeSystem.API/", "src/WorkIntakeSystem.API/"]
COPY ["src/WorkIntakeSystem.Core/", "src/WorkIntakeSystem.Core/"]
COPY ["src/WorkIntakeSystem.Infrastructure/", "src/WorkIntakeSystem.Infrastructure/"]

# Build the application
WORKDIR "/src/src/WorkIntakeSystem.API"
RUN dotnet build "WorkIntakeSystem.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "WorkIntakeSystem.API.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false \
    /p:DebugType=None \
    /p:DebugSymbols=false

# Remove unnecessary localization files and runtime packages
RUN find /app/publish -type d -name "cs" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "de" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "es" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "fr" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "it" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "ja" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "ko" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "pl" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "pt-BR" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "ru" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "tr" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "zh-Hans" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish -type d -name "zh-Hant" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish/runtimes -type d -name "linux-arm*" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish/runtimes -type d -name "osx*" -exec rm -rf {} + 2>/dev/null || true && \
    find /app/publish/runtimes -type d -name "win-arm*" -exec rm -rf {} + 2>/dev/null || true

# Final stage/image
FROM base AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy the published application
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown appuser /app/logs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "WorkIntakeSystem.API.dll"] 