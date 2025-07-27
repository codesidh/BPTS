# Use the official .NET 8.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files
COPY ["src/WorkIntakeSystem.API/WorkIntakeSystem.API.csproj", "src/WorkIntakeSystem.API/"]
COPY ["src/WorkIntakeSystem.Core/WorkIntakeSystem.Core.csproj", "src/WorkIntakeSystem.Core/"]
COPY ["src/WorkIntakeSystem.Infrastructure/WorkIntakeSystem.Infrastructure.csproj", "src/WorkIntakeSystem.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/WorkIntakeSystem.API/WorkIntakeSystem.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/WorkIntakeSystem.API"
RUN dotnet build "WorkIntakeSystem.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "WorkIntakeSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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