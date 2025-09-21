# 🚀 Work Intake System - Docker Deployment Guide

This directory contains automated deployment scripts for running the Work Intake System in Docker containers.

## 📋 Prerequisites

- **Docker Desktop** installed and running
- **Windows 10/11** with PowerShell 5.1+ or PowerShell Core
- **8GB+ RAM** recommended for all services
- **Ports Available**: 1433 (SQL Server), 3000 (Web), 5000 (API), 6379 (Redis)

## 🎯 Quick Start

### Option 1: PowerShell Script (Recommended)
```powershell
# Standard deployment
.\deploy-docker.ps1

# Clean deployment (removes all existing containers/images)
.\deploy-docker.ps1 -Clean

# Show help
.\deploy-docker.ps1 -Help
```

### Option 2: Batch Script (Windows Compatibility)
```cmd
# Standard deployment
deploy-docker.bat

# Clean deployment
deploy-docker.bat --clean

# Show help
deploy-docker.bat --help
```

## 📁 Deployment Files

| File | Purpose |
|------|---------|
| `deploy-docker.ps1` | Main PowerShell deployment script with full automation |
| `deploy-docker.bat` | Batch file alternative for compatibility |
| `init-database.sql` | Database initialization script |
| `.dockerignore` | Excludes unnecessary files from Docker build context |
| `docker-compose.yml` | Docker services configuration |

## 🔧 Script Options

### PowerShell Script Options
- `-Clean` - Remove all existing containers, images, and volumes
- `-SkipBuild` - Skip Docker image building (use existing images)
- `-SkipDatabase` - Skip database initialization
- `-Verbose` - Show detailed output
- `-Help` - Display help information

### Batch Script Options
- `--clean` - Remove all existing containers, images, and volumes
- `--skip-build` - Skip Docker image building
- `--skip-database` - Skip database initialization
- `--help` - Display help information

## 🏗️ What Gets Deployed

| Service | Port | Description | Status |
|---------|------|-------------|---------|
| **SQL Server 2022** | 1433 | Database server | ✅ Fully functional |
| **Redis Cache** | 6379 | Caching and session storage | ✅ Fully functional |
| **Web Application** | 3000 | React-based frontend | ✅ Fully functional |
| **REST API** | 5000 | .NET 8 Web API | ⚠️ May need configuration |

## 🔗 Access Points

After successful deployment:

- **Web Application**: http://localhost:3000
- **API Health Check**: http://localhost:5000/health
- **Database**: `localhost:1433` (sa/YourStrong@Passw0rd)
- **Redis**: `localhost:6379`

## 📊 Deployment Process

The script follows this sequence:

1. **Prerequisites Check** - Verify Docker is running
2. **Cleanup** (if -Clean flag) - Remove existing containers/images
3. **Build Artifacts Cleanup** - Remove bin/obj/node_modules folders
4. **Database Services** - Start SQL Server and Redis
5. **Database Initialization** - Create database and configure settings
6. **Image Building** - Build API and Web application images
7. **Service Startup** - Start all application services
8. **Health Checks** - Verify services are responding
9. **Summary Report** - Show deployment status and access URLs

## 🛠️ Troubleshooting

### Common Issues

#### Docker Not Running
```
❌ Docker Desktop is not running
```
**Solution**: Start Docker Desktop and wait for it to be ready.

#### Port Conflicts
```
❌ Port 3000 already in use
```
**Solution**: Stop other services using these ports or modify `docker-compose.yml`.

#### API Service Issues
```
⚠️ API may need additional configuration
```
**Solution**: Check API logs:
```bash
docker-compose logs workintake-api
```

#### Database Connection Issues
```
❌ Failed to connect to database
```
**Solution**: Ensure SQL Server container is healthy:
```bash
docker-compose ps
docker-compose logs sqlserver
```

### Manual Commands

If the script fails, you can run individual steps:

```bash
# Stop all services
docker-compose down --remove-orphans

# Start database services only
docker-compose up -d sqlserver redis

# Build specific service
docker-compose build workintake-api

# Check service logs
docker-compose logs [service-name]

# Check container status
docker-compose ps
```

### Database Manual Initialization

If database initialization fails:

```bash
# Connect to SQL Server container
docker exec -it bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C

# Or run the initialization script
docker exec -i bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C < init-database.sql
```

## 🔍 Monitoring

### Check Service Status
```bash
# View all containers
docker-compose ps

# View specific service logs
docker-compose logs -f workintake-web
docker-compose logs -f workintake-api

# View resource usage
docker stats
```

### Health Checks
```bash
# Web application
curl -I http://localhost:3000

# API health endpoint
curl -I http://localhost:5000/health

# Database connection
docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT @@VERSION"
```

## 🧹 Cleanup

### Stop Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down --volumes
```

### Complete Cleanup
```bash
# Using PowerShell script
.\deploy-docker.ps1 -Clean

# Or manually
docker-compose down --volumes --remove-orphans
docker system prune -a -f --volumes
```

## 📈 Performance Tips

1. **Allocate sufficient resources** to Docker Desktop (8GB+ RAM recommended)
2. **Use SSD storage** for Docker volumes for better database performance
3. **Close unnecessary applications** during deployment to free up resources
4. **Enable WSL2 backend** in Docker Desktop for better performance on Windows

## 🔒 Security Notes

- Default SQL Server password is `YourStrong@Passw0rd` - change this in production
- Services are exposed on localhost only by default
- No SSL/TLS configured - add certificates for production use
- Consider using Docker secrets for production deployments

## 📞 Support

If you encounter issues:

1. Check the troubleshooting section above
2. Review Docker Desktop logs
3. Ensure all prerequisites are met
4. Try running with the `-Clean` flag for a fresh start
5. Check container logs for specific error messages

---

**Note**: This deployment is configured for development/testing purposes. For production deployment, additional security, monitoring, and backup configurations are recommended.
