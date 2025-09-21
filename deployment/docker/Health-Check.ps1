#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Health check script for BPTS Work Intake System Docker deployment
.DESCRIPTION
    This script performs comprehensive health checks on all services in the Docker deployment
.PARAMETER Detailed
    Show detailed health information for each service
.PARAMETER Json
    Output results in JSON format
.PARAMETER ContinuousMonitoring
    Run health checks continuously with specified interval
.PARAMETER Interval
    Interval in seconds for continuous monitoring (default: 30)
.EXAMPLE
    .\Health-Check.ps1
.EXAMPLE
    .\Health-Check.ps1 -Detailed -Json
.EXAMPLE
    .\Health-Check.ps1 -ContinuousMonitoring -Interval 60
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Detailed,
    
    [Parameter(Mandatory = $false)]
    [switch]$Json,
    
    [Parameter(Mandatory = $false)]
    [switch]$ContinuousMonitoring,
    
    [Parameter(Mandatory = $false)]
    [int]$Interval = 30
)

# Script configuration
$ErrorActionPreference = "Continue"

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

function Write-ColoredOutput {
    param([string]$Message, [string]$Color = $Reset)
    if (-not $Json) {
        Write-Host "${Color}$Message${Reset}"
    }
}

function Test-ServiceHealth {
    param(
        [string]$ServiceName,
        [string]$Url = $null,
        [string]$Command = $null,
        [int]$ExpectedStatusCode = 200,
        [int]$TimeoutSeconds = 10
    )
    
    $healthResult = @{
        ServiceName = $ServiceName
        Status = "Unknown"
        ResponseTime = $null
        StatusCode = $null
        ErrorMessage = $null
        LastChecked = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
        Details = @{}
    }
    
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        
        if ($Url) {
            # HTTP health check
            $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec $TimeoutSeconds -ErrorAction Stop
            $stopwatch.Stop()
            
            $healthResult.ResponseTime = $stopwatch.ElapsedMilliseconds
            $healthResult.StatusCode = $response.StatusCode
            
            if ($response.StatusCode -eq $ExpectedStatusCode) {
                $healthResult.Status = "Healthy"
            } else {
                $healthResult.Status = "Unhealthy"
                $healthResult.ErrorMessage = "Unexpected status code: $($response.StatusCode)"
            }
            
            if ($Detailed) {
                $healthResult.Details.ContentLength = $response.Content.Length
                $healthResult.Details.Headers = $response.Headers
            }
        }
        elseif ($Command) {
            # Command-based health check
            $result = Invoke-Expression $Command 2>&1
            $stopwatch.Stop()
            
            $healthResult.ResponseTime = $stopwatch.ElapsedMilliseconds
            
            if ($LASTEXITCODE -eq 0) {
                $healthResult.Status = "Healthy"
                if ($Detailed) {
                    $healthResult.Details.CommandOutput = $result
                }
            } else {
                $healthResult.Status = "Unhealthy"
                $healthResult.ErrorMessage = "Command failed with exit code: $LASTEXITCODE"
                $healthResult.Details.CommandOutput = $result
            }
        }
    }
    catch {
        $stopwatch.Stop()
        $healthResult.Status = "Unhealthy"
        $healthResult.ErrorMessage = $_.Exception.Message
        $healthResult.ResponseTime = $stopwatch.ElapsedMilliseconds
    }
    
    return $healthResult
}

function Get-ContainerHealth {
    param([string]$ContainerName)
    
    try {
        $containerInfo = docker inspect $ContainerName 2>$null | ConvertFrom-Json
        
        if ($containerInfo) {
            $state = $containerInfo[0].State
            return @{
                Status = $state.Status
                Health = $state.Health.Status
                Running = $state.Running
                StartedAt = $state.StartedAt
                ExitCode = $state.ExitCode
            }
        }
    }
    catch {
        return @{
            Status = "NotFound"
            Health = "Unknown"
            Running = $false
            Error = $_.Exception.Message
        }
    }
    
    return $null
}

function Test-AllServices {
    $services = @(
        @{
            Name = "Web Application"
            Url = "http://localhost:3000"
            Container = "bpts-workintake-web-1"
        },
        @{
            Name = "API Service"
            Url = "http://localhost:5000/health"
            Container = "bpts-workintake-api-1"
        },
        @{
            Name = "SQL Server"
            Command = 'docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT 1" 2>$null'
            Container = "bpts-sqlserver-1"
        },
        @{
            Name = "Redis"
            Command = "docker exec bpts-redis-1 redis-cli ping 2>$null"
            Container = "bpts-redis-1"
        }
    )
    
    $results = @()
    
    foreach ($service in $services) {
        Write-ColoredOutput "Checking $($service.Name)..." $Cyan
        
        # Get service health
        $healthCheck = Test-ServiceHealth -ServiceName $service.Name -Url $service.Url -Command $service.Command
        
        # Get container information
        if ($service.Container) {
            $containerHealth = Get-ContainerHealth $service.Container
            $healthCheck.Details.Container = $containerHealth
        }
        
        $results += $healthCheck
        
        # Display result
        if (-not $Json) {
            $statusColor = switch ($healthCheck.Status) {
                "Healthy" { $Green }
                "Unhealthy" { $Red }
                default { $Yellow }
            }
            
            $statusText = "[$($healthCheck.Status)]"
            if ($healthCheck.ResponseTime) {
                $statusText += " ($($healthCheck.ResponseTime)ms)"
            }
            
            Write-ColoredOutput "  $($service.Name): $statusText" $statusColor
            
            if ($healthCheck.ErrorMessage) {
                Write-ColoredOutput "    Error: $($healthCheck.ErrorMessage)" $Red
            }
            
            if ($Detailed -and $healthCheck.Details.Container) {
                $container = $healthCheck.Details.Container
                Write-ColoredOutput "    Container Status: $($container.Status)" $Cyan
                if ($container.Health) {
                    Write-ColoredOutput "    Container Health: $($container.Health)" $Cyan
                }
            }
        }
    }
    
    return $results
}

function Show-Summary {
    param([array]$Results)
    
    $healthyCount = ($Results | Where-Object { $_.Status -eq "Healthy" }).Count
    $unhealthyCount = ($Results | Where-Object { $_.Status -eq "Unhealthy" }).Count
    $totalCount = $Results.Count
    
    if (-not $Json) {
        Write-ColoredOutput "`n=== Health Check Summary ===" $Cyan
        Write-ColoredOutput "Total Services: $totalCount" $Cyan
        Write-ColoredOutput "Healthy: $healthyCount" $Green
        Write-ColoredOutput "Unhealthy: $unhealthyCount" $(if ($unhealthyCount -gt 0) { $Red } else { $Green })
        
        $overallStatus = if ($unhealthyCount -eq 0) { "All services are healthy" } else { "Some services need attention" }
        $overallColor = if ($unhealthyCount -eq 0) { $Green } else { $Yellow }
        
        Write-ColoredOutput "`nOverall Status: $overallStatus" $overallColor
        
        if ($unhealthyCount -gt 0) {
            Write-ColoredOutput "`nUnhealthy Services:" $Red
            $Results | Where-Object { $_.Status -eq "Unhealthy" } | ForEach-Object {
                Write-ColoredOutput "  - $($_.ServiceName): $($_.ErrorMessage)" $Red
            }
            
            Write-ColoredOutput "`nTroubleshooting Commands:" $Yellow
            Write-ColoredOutput "  View logs: docker-compose logs -f [service-name]" $Yellow
            Write-ColoredOutput "  Restart service: docker-compose restart [service-name]" $Yellow
            Write-ColoredOutput "  Check containers: docker ps -a" $Yellow
        }
    }
    
    return @{
        Total = $totalCount
        Healthy = $healthyCount
        Unhealthy = $unhealthyCount
        OverallHealthy = ($unhealthyCount -eq 0)
    }
}

function Start-HealthCheck {
    do {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
        
        if (-not $Json) {
            Clear-Host
            Write-ColoredOutput "=== BPTS Work Intake System Health Check ===" $Cyan
            Write-ColoredOutput "Timestamp: $timestamp" $Cyan
            Write-ColoredOutput ""
        }
        
        # Run health checks
        $results = Test-AllServices
        
        # Show summary
        $summary = Show-Summary $results
        
        # Output results
        if ($Json) {
            $output = @{
                Timestamp = $timestamp
                Summary = $summary
                Services = $results
            }
            $output | ConvertTo-Json -Depth 10
        }
        
        # Exit if not continuous monitoring
        if (-not $ContinuousMonitoring) {
            break
        }
        
        # Wait for next check
        if (-not $Json) {
            Write-ColoredOutput "`nNext check in $Interval seconds... (Press Ctrl+C to stop)" $Yellow
        }
        
        Start-Sleep -Seconds $Interval
        
    } while ($ContinuousMonitoring)
    
    # Return exit code based on health
    if ($summary.Unhealthy -gt 0) {
        exit 1
    } else {
        exit 0
    }
}

# Main execution
Start-HealthCheck
