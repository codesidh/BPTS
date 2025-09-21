#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Docker deployment script for BPTS Work Intake System
.DESCRIPTION
    This script handles the complete Docker deployment of the Work Intake System,
    including database setup, application containers, and health checks.
.PARAMETER Environment
    The deployment environment (Development, Staging, Production)
.PARAMETER SkipBuild
    Skip building images and use existing ones
.PARAMETER SkipDatabase
    Skip database initialization
.PARAMETER Monitoring
    Include monitoring stack (ELK)
.PARAMETER CleanStart
    Clean all existing containers and volumes before deployment
.EXAMPLE
    .\Deploy-Docker.ps1 -Environment Development
.EXAMPLE
    .\Deploy-Docker.ps1 -Environment Production -Monitoring
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment = "Development",
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipDatabase,
    
    [Parameter(Mandatory = $false)]
    [switch]$Monitoring,
    
    [Parameter(Mandatory = $false)]
    [switch]$CleanStart,
    
    [Parameter(Mandatory = $false)]
    [switch]$WaitForHealthy
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Colors for output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Magenta = "`e[35m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

# Deployment configuration
$DeploymentConfig = @{
    ProjectName = "bpts"
    ComposeFile = "docker-compose.yml"
    MonitoringFile = "docker-compose.monitoring.yml"
    DatabaseName = "WorkIntakeSystem"
    DatabasePassword = "YourStrong@Passw0rd"
    HealthCheckTimeout = 300
    BuildTimeout = 600
}

function Write-ColoredOutput {
    param(
        [string]$Message,
        [string]$Color = $Reset,
        [string]$Type = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "${Color}[$timestamp] [$Type] $Message${Reset}"
}

function Write-Info { param([string]$Message) Write-ColoredOutput $Message $Cyan "INFO" }
function Write-Success { param([string]$Message) Write-ColoredOutput $Message $Green "SUCCESS" }
function Write-Warning { param([string]$Message) Write-ColoredOutput $Message $Yellow "WARNING" }
function Write-Error { param([string]$Message) Write-ColoredOutput $Message $Red "ERROR" }

function Test-DockerInstallation {
    Write-Info "Checking Docker installation..."
    
    try {
        $dockerVersion = docker --version
        Write-Success "Docker found: $dockerVersion"
        
        $composeVersion = docker-compose --version
        Write-Success "Docker Compose found: $composeVersion"
        
        # Test Docker daemon
        docker info | Out-Null
        Write-Success "Docker daemon is running"
        
        return $true
    }
    catch {
        Write-Error "Docker is not properly installed or running: $_"
        return $false
    }
}

function Stop-ExistingContainers {
    Write-Info "Stopping existing containers..."
    
    try {
        if ($Monitoring) {
            docker-compose -f $DeploymentConfig.MonitoringFile down --remove-orphans 2>$null
        }
        
        docker-compose -f $DeploymentConfig.ComposeFile down --remove-orphans
        Write-Success "Existing containers stopped"
        
        if ($CleanStart) {
            Write-Warning "Cleaning up volumes and networks..."
            docker-compose -f $DeploymentConfig.ComposeFile down --volumes --remove-orphans
            docker system prune -f
            Write-Success "Clean start completed"
        }
    }
    catch {
        Write-Warning "Error stopping containers (may not exist): $_"
    }
}

function Build-DockerImages {
    if ($SkipBuild) {
        Write-Info "Skipping image build as requested"
        return $true
    }
    
    Write-Info "Building Docker images..."
    
    try {
        # Build API image
        Write-Info "Building API image..."
        docker-compose -f $DeploymentConfig.ComposeFile build workintake-api
        Write-Success "API image built successfully"
        
        # Build Web image
        Write-Info "Building Web application image..."
        docker-compose -f $DeploymentConfig.ComposeFile build workintake-web
        Write-Success "Web application image built successfully"
        
        return $true
    }
    catch {
        Write-Error "Failed to build images: $_"
        return $false
    }
}

function Start-DatabaseServices {
    Write-Info "Starting database services..."
    
    try {
        # Start SQL Server and Redis
        docker-compose -f $DeploymentConfig.ComposeFile up -d sqlserver redis
        Write-Success "Database services started"
        
        # Wait for SQL Server to be ready
        Write-Info "Waiting for SQL Server to initialize..."
        $timeout = 60
        $elapsed = 0
        
        do {
            Start-Sleep -Seconds 5
            $elapsed += 5
            Write-Info "Waiting for SQL Server... ($elapsed/$timeout seconds)"
            
            try {
                $result = docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $DeploymentConfig.DatabasePassword -C -Q "SELECT 1" 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "SQL Server is ready"
                    break
                }
            }
            catch {
                # Continue waiting
            }
        } while ($elapsed -lt $timeout)
        
        if ($elapsed -ge $timeout) {
            Write-Warning "SQL Server may not be fully ready, but continuing..."
        }
        
        return $true
    }
    catch {
        Write-Error "Failed to start database services: $_"
        return $false
    }
}

function Initialize-Database {
    if ($SkipDatabase) {
        Write-Info "Skipping database initialization as requested"
        return $true
    }
    
    Write-Info "Initializing database..."
    
    try {
        # Create database if it doesn't exist
        $createDbQuery = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$($DeploymentConfig.DatabaseName)')
BEGIN
    CREATE DATABASE [$($DeploymentConfig.DatabaseName)];
    PRINT 'Database created successfully';
END
ELSE
BEGIN
    PRINT 'Database already exists';
END
"@
        
        docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $DeploymentConfig.DatabasePassword -C -Q $createDbQuery
        Write-Success "Database initialization completed"
        
        return $true
    }
    catch {
        Write-Error "Failed to initialize database: $_"
        return $false
    }
}

function Start-ApplicationServices {
    Write-Info "Starting application services..."
    
    try {
        # Start API
        Write-Info "Starting API service..."
        docker-compose -f $DeploymentConfig.ComposeFile up -d workintake-api
        
        # Start Web application
        Write-Info "Starting Web application..."
        docker-compose -f $DeploymentConfig.ComposeFile up -d workintake-web
        
        Write-Success "Application services started"
        return $true
    }
    catch {
        Write-Error "Failed to start application services: $_"
        return $false
    }
}

function Start-MonitoringServices {
    if (-not $Monitoring) {
        Write-Info "Monitoring services not requested"
        return $true
    }
    
    Write-Info "Starting monitoring services..."
    
    try {
        docker-compose -f $DeploymentConfig.MonitoringFile up -d
        Write-Success "Monitoring services started"
        return $true
    }
    catch {
        Write-Error "Failed to start monitoring services: $_"
        return $false
    }
}

function Test-ServiceHealth {
    Write-Info "Performing health checks..."
    
    $services = @(
        @{ Name = "Web Application"; Url = "http://localhost:3000"; ExpectedStatus = 200 }
        @{ Name = "Redis"; Command = "docker exec bpts-redis-1 redis-cli ping" }
    )
    
    $allHealthy = $true
    
    foreach ($service in $services) {
        Write-Info "Checking $($service.Name)..."
        
        try {
            if ($service.Url) {
                $response = Invoke-WebRequest -Uri $service.Url -Method HEAD -TimeoutSec 10 -ErrorAction Stop
                if ($response.StatusCode -eq $service.ExpectedStatus) {
                    Write-Success "$($service.Name) is healthy"
                } else {
                    Write-Warning "$($service.Name) returned status code $($response.StatusCode)"
                    $allHealthy = $false
                }
            }
            elseif ($service.Command) {
                Invoke-Expression $service.Command | Out-Null
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "$($service.Name) is healthy"
                } else {
                    Write-Warning "$($service.Name) health check failed"
                    $allHealthy = $false
                }
            }
        }
        catch {
            Write-Warning "$($service.Name) health check failed: $_"
            $allHealthy = $false
        }
    }
    
    return $allHealthy
}

function Show-DeploymentSummary {
    Write-Info "Deployment Summary"
    Write-Info "=================="
    
    try {
        $containers = docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Select-String -Pattern "bpts"
        
        Write-Info "Running Containers:"
        foreach ($container in $containers) {
            Write-Info "  $container"
        }
        
        Write-Info ""
        Write-Info "Service URLs:"
        Write-Success "  Web Application: http://localhost:3000"
        Write-Success "  Database: localhost:1433 (sa/YourStrong@Passw0rd)"
        Write-Success "  Redis: localhost:6379"
        
        if ($Monitoring) {
            Write-Success "  Kibana: http://localhost:5601"
            Write-Success "  Elasticsearch: http://localhost:9200"
        }
        
        Write-Info ""
        Write-Info "Useful Commands:"
        Write-Info "  View logs: docker-compose logs -f [service-name]"
        Write-Info "  Stop all: docker-compose down"
        Write-Info "  Restart: docker-compose restart [service-name]"
        
    }
    catch {
        Write-Warning "Could not generate deployment summary: $_"
    }
}

function Wait-ForHealthyServices {
    if (-not $WaitForHealthy) {
        return
    }
    
    Write-Info "Waiting for all services to be healthy..."
    $timeout = $DeploymentConfig.HealthCheckTimeout
    $elapsed = 0
    $checkInterval = 10
    
    do {
        Start-Sleep -Seconds $checkInterval
        $elapsed += $checkInterval
        Write-Info "Health check in progress... ($elapsed/$timeout seconds)"
        
        if (Test-ServiceHealth) {
            Write-Success "All services are healthy!"
            return
        }
    } while ($elapsed -lt $timeout)
    
    Write-Warning "Health check timeout reached. Some services may not be fully ready."
}

# Main deployment process
function Start-Deployment {
    Write-Info "Starting BPTS Docker deployment..."
    Write-Info "Environment: $Environment"
    Write-Info "Skip Build: $SkipBuild"
    Write-Info "Skip Database: $SkipDatabase"
    Write-Info "Include Monitoring: $Monitoring"
    Write-Info "Clean Start: $CleanStart"
    Write-Info ""
    
    # Pre-flight checks
    if (-not (Test-DockerInstallation)) {
        exit 1
    }
    
    # Change to project root directory
    $scriptPath = $PSScriptRoot
    if (-not $scriptPath) {
        $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
    }
    $projectRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
    Set-Location $projectRoot
    Write-Info "Working directory: $(Get-Location)"
    
    try {
        # Deployment steps
        Stop-ExistingContainers
        
        if (-not (Build-DockerImages)) { exit 1 }
        if (-not (Start-DatabaseServices)) { exit 1 }
        if (-not (Initialize-Database)) { exit 1 }
        if (-not (Start-ApplicationServices)) { exit 1 }
        if (-not (Start-MonitoringServices)) { exit 1 }
        
        # Wait for services to be ready
        Start-Sleep -Seconds 10
        Wait-ForHealthyServices
        
        # Final health check and summary
        Test-ServiceHealth | Out-Null
        Show-DeploymentSummary
        
        Write-Success "Deployment completed successfully!"
        Write-Info "You can now access the application at http://localhost:3000"
        
    }
    catch {
        Write-Error "Deployment failed: $_"
        Write-Error "Check the logs with: docker-compose logs"
        exit 1
    }
}

# Execute deployment
Start-Deployment
