#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run Entity Framework migrations for the Work Intake System
.DESCRIPTION
    This script runs Entity Framework database migrations against the containerized SQL Server
.PARAMETER ConnectionString
    Override the default connection string
.PARAMETER Environment
    Target environment (Development, Staging, Production)
.EXAMPLE
    .\Run-Migrations.ps1
.EXAMPLE
    .\Run-Migrations.ps1 -Environment Production
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString = "Server=localhost,1433;Database=WorkIntakeSystem;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment = "Development"
)

# Script configuration
$ErrorActionPreference = "Stop"

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

function Write-Info { param([string]$Message) Write-Host "${Cyan}[INFO] $Message${Reset}" }
function Write-Success { param([string]$Message) Write-Host "${Green}[SUCCESS] $Message${Reset}" }
function Write-Warning { param([string]$Message) Write-Host "${Yellow}[WARNING] $Message${Reset}" }
function Write-Error { param([string]$Message) Write-Host "${Red}[ERROR] $Message${Reset}" }

function Test-DatabaseConnection {
    param([string]$ConnString)
    
    Write-Info "Testing database connection..."
    
    try {
        # Test using SQL Server container
        $testQuery = "SELECT 1"
        docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q $testQuery | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Database connection successful"
            return $true
        } else {
            Write-Error "Database connection failed"
            return $false
        }
    }
    catch {
        Write-Error "Database connection test failed: $_"
        return $false
    }
}

function Run-EFMigrations {
    Write-Info "Running Entity Framework migrations..."
    
    # Change to project root
    $scriptPath = $PSScriptRoot
    if (-not $scriptPath) {
        $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
    }
    $projectRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
    $apiProject = Join-Path $projectRoot "src\WorkIntakeSystem.API"
    
    if (-not (Test-Path $apiProject)) {
        Write-Error "API project not found at: $apiProject"
        return $false
    }
    
    Set-Location $projectRoot
    Write-Info "Working directory: $(Get-Location)"
    
    try {
        # Set environment variables
        $env:ASPNETCORE_ENVIRONMENT = $Environment
        $env:ConnectionStrings__DefaultConnection = $ConnectionString
        
        Write-Info "Checking for pending migrations..."
        $pendingMigrations = dotnet ef migrations list --project "src\WorkIntakeSystem.Infrastructure" --startup-project "src\WorkIntakeSystem.API" --no-build 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Could not check migrations status. Attempting to run migrations anyway..."
        }
        
        Write-Info "Applying database migrations..."
        $migrationResult = dotnet ef database update --project "src\WorkIntakeSystem.Infrastructure" --startup-project "src\WorkIntakeSystem.API" --no-build --verbose 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Database migrations completed successfully"
            Write-Info "Migration output:"
            $migrationResult | ForEach-Object { Write-Info "  $_" }
            return $true
        } else {
            Write-Error "Database migration failed"
            Write-Error "Migration output:"
            $migrationResult | ForEach-Object { Write-Error "  $_" }
            return $false
        }
    }
    catch {
        Write-Error "Failed to run migrations: $_"
        return $false
    }
}

function Initialize-SeedData {
    Write-Info "Checking for seed data initialization..."
    
    try {
        # This would typically involve running a seed data script or calling an API endpoint
        # For now, we'll just verify the database is accessible
        
        $verifyQuery = @"
SELECT 
    COUNT(*) as TableCount
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_SCHEMA = 'dbo'
"@
        
        $result = docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -d WorkIntakeSystem -C -Q $verifyQuery
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Database schema verification completed"
            Write-Info "Result: $result"
            return $true
        } else {
            Write-Warning "Could not verify database schema"
            return $false
        }
    }
    catch {
        Write-Warning "Seed data initialization failed: $_"
        return $false
    }
}

# Main execution
function Start-MigrationProcess {
    Write-Info "Starting Entity Framework migration process..."
    Write-Info "Environment: $Environment"
    Write-Info "Connection: $($ConnectionString -replace 'Password=.*?;', 'Password=***;')"
    Write-Info ""
    
    try {
        # Test database connection
        if (-not (Test-DatabaseConnection $ConnectionString)) {
            Write-Error "Cannot proceed without database connection"
            exit 1
        }
        
        # Run migrations
        if (-not (Run-EFMigrations)) {
            Write-Error "Migration process failed"
            exit 1
        }
        
        # Initialize seed data
        Initialize-SeedData | Out-Null
        
        Write-Success "Migration process completed successfully!"
        Write-Info "The database is now ready for the application."
        
    }
    catch {
        Write-Error "Migration process failed: $_"
        exit 1
    }
}

# Execute migration process
Start-MigrationProcess
