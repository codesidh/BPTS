#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Service monitoring script for BPTS Work Intake System
.DESCRIPTION
    This script provides real-time monitoring of Docker services with logging and alerting
.PARAMETER LogFile
    Path to log file for monitoring results
.PARAMETER AlertThreshold
    Number of consecutive failures before alerting (default: 3)
.PARAMETER CheckInterval
    Interval in seconds between checks (default: 30)
.PARAMETER EmailAlerts
    Enable email alerts for service failures
.PARAMETER WebhookUrl
    Webhook URL for notifications (Slack, Teams, etc.)
.EXAMPLE
    .\Monitor-Services.ps1
.EXAMPLE
    .\Monitor-Services.ps1 -LogFile "logs\monitoring.log" -EmailAlerts
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$LogFile = "logs\service-monitoring.log",
    
    [Parameter(Mandatory = $false)]
    [int]$AlertThreshold = 3,
    
    [Parameter(Mandatory = $false)]
    [int]$CheckInterval = 30,
    
    [Parameter(Mandatory = $false)]
    [switch]$EmailAlerts,
    
    [Parameter(Mandatory = $false)]
    [string]$WebhookUrl
)

# Script configuration
$ErrorActionPreference = "Continue"

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

# Global variables for tracking
$script:ServiceFailures = @{}
$script:LastAlertTime = @{}

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO",
        [string]$Color = $Reset
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    
    # Write to console
    Write-Host "${Color}$logEntry${Reset}"
    
    # Write to log file
    if ($LogFile) {
        try {
            $logDir = Split-Path -Parent $LogFile
            if ($logDir -and -not (Test-Path $logDir)) {
                New-Item -ItemType Directory -Path $logDir -Force | Out-Null
            }
            Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
        }
        catch {
            Write-Host "${Red}Failed to write to log file: $_${Reset}"
        }
    }
}

function Send-Alert {
    param(
        [string]$ServiceName,
        [string]$AlertType,
        [string]$Message
    )
    
    $alertKey = "$ServiceName-$AlertType"
    $now = Get-Date
    
    # Check if we've sent an alert recently (prevent spam)
    if ($script:LastAlertTime.ContainsKey($alertKey)) {
        $timeSinceLastAlert = $now - $script:LastAlertTime[$alertKey]
        if ($timeSinceLastAlert.TotalMinutes -lt 15) {
            Write-Log "Skipping alert for $ServiceName (too recent)" "DEBUG" $Yellow
            return
        }
    }
    
    $script:LastAlertTime[$alertKey] = $now
    
    Write-Log "ALERT: $ServiceName - $Message" "ALERT" $Red
    
    # Send webhook notification
    if ($WebhookUrl) {
        try {
            $payload = @{
                text = "🚨 BPTS Service Alert"
                attachments = @(
                    @{
                        color = "danger"
                        fields = @(
                            @{
                                title = "Service"
                                value = $ServiceName
                                short = $true
                            },
                            @{
                                title = "Alert Type"
                                value = $AlertType
                                short = $true
                            },
                            @{
                                title = "Message"
                                value = $Message
                                short = $false
                            },
                            @{
                                title = "Timestamp"
                                value = $now.ToString("yyyy-MM-dd HH:mm:ss UTC")
                                short = $true
                            }
                        )
                    }
                )
            }
            
            Invoke-RestMethod -Uri $WebhookUrl -Method POST -Body ($payload | ConvertTo-Json -Depth 10) -ContentType "application/json" | Out-Null
            Write-Log "Webhook alert sent successfully" "DEBUG" $Green
        }
        catch {
            Write-Log "Failed to send webhook alert: $_" "ERROR" $Red
        }
    }
    
    # Send email alert (if configured)
    if ($EmailAlerts) {
        # Email configuration would go here
        Write-Log "Email alerts not yet implemented" "DEBUG" $Yellow
    }
}

function Test-ServiceHealth {
    param([string]$ServiceName, [string]$Url, [string]$Command)
    
    try {
        if ($Url) {
            $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10 -ErrorAction Stop
            return @{
                Status = "Healthy"
                ResponseTime = 0
                Details = "HTTP $($response.StatusCode)"
            }
        }
        elseif ($Command) {
            Invoke-Expression $Command 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                return @{
                    Status = "Healthy"
                    ResponseTime = 0
                    Details = "Command executed successfully"
                }
            } else {
                return @{
                    Status = "Unhealthy"
                    ResponseTime = 0
                    Details = "Command failed with exit code $LASTEXITCODE"
                }
            }
        }
    }
    catch {
        return @{
            Status = "Unhealthy"
            ResponseTime = 0
            Details = $_.Exception.Message
        }
    }
}

function Monitor-Service {
    param([hashtable]$Service)
    
    $result = Test-ServiceHealth -ServiceName $Service.Name -Url $Service.Url -Command $Service.Command
    
    # Track failures
    if ($result.Status -eq "Unhealthy") {
        if (-not $script:ServiceFailures.ContainsKey($Service.Name)) {
            $script:ServiceFailures[$Service.Name] = 0
        }
        $script:ServiceFailures[$Service.Name]++
        
        Write-Log "$($Service.Name) is unhealthy: $($result.Details)" "ERROR" $Red
        
        # Send alert if threshold reached
        if ($script:ServiceFailures[$Service.Name] -eq $AlertThreshold) {
            Send-Alert -ServiceName $Service.Name -AlertType "SERVICE_DOWN" -Message "Service has failed $AlertThreshold consecutive health checks. Details: $($result.Details)"
        }
    } else {
        # Service is healthy
        if ($script:ServiceFailures.ContainsKey($Service.Name) -and $script:ServiceFailures[$Service.Name] -gt 0) {
            # Service recovered
            $previousFailures = $script:ServiceFailures[$Service.Name]
            $script:ServiceFailures[$Service.Name] = 0
            
            Write-Log "$($Service.Name) has recovered after $previousFailures failures" "INFO" $Green
            Send-Alert -ServiceName $Service.Name -AlertType "SERVICE_RECOVERED" -Message "Service has recovered after $previousFailures consecutive failures"
        }
        
        Write-Log "$($Service.Name) is healthy: $($result.Details)" "INFO" $Green
    }
    
    return $result
}

function Get-SystemMetrics {
    try {
        # Get Docker system information
        $dockerInfo = docker system df --format "table {{.Type}}\t{{.Total}}\t{{.Active}}\t{{.Size}}" 2>$null
        
        # Get container resource usage
        $containerStats = docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}" 2>$null
        
        return @{
            DockerInfo = $dockerInfo
            ContainerStats = $containerStats
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
        }
    }
    catch {
        Write-Log "Failed to get system metrics: $_" "WARNING" $Yellow
        return $null
    }
}

function Show-MonitoringStatus {
    param([array]$Results, [hashtable]$SystemMetrics)
    
    Clear-Host
    Write-Host "${Cyan}=== BPTS Service Monitoring Dashboard ===${Reset}"
    Write-Host "${Cyan}Last Update: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')${Reset}"
    Write-Host "${Cyan}Check Interval: $CheckInterval seconds${Reset}"
    Write-Host "${Cyan}Alert Threshold: $AlertThreshold failures${Reset}"
    Write-Host ""
    
    # Service status
    Write-Host "${Cyan}=== Service Status ===${Reset}"
    foreach ($result in $Results) {
        $statusColor = if ($result.Status -eq "Healthy") { $Green } else { $Red }
        $failureCount = if ($script:ServiceFailures.ContainsKey($result.ServiceName)) { $script:ServiceFailures[$result.ServiceName] } else { 0 }
        
        Write-Host "${statusColor}$($result.ServiceName): $($result.Status)${Reset}"
        if ($failureCount -gt 0) {
            Write-Host "  ${Red}Consecutive Failures: $failureCount${Reset}"
        }
        Write-Host "  Details: $($result.Details)"
    }
    
    # System metrics
    if ($SystemMetrics) {
        Write-Host ""
        Write-Host "${Cyan}=== System Metrics ===${Reset}"
        if ($SystemMetrics.ContainerStats) {
            Write-Host "Container Resource Usage:"
            $SystemMetrics.ContainerStats | ForEach-Object { Write-Host "  $_" }
        }
    }
    
    # Failure summary
    $totalFailures = ($script:ServiceFailures.Values | Measure-Object -Sum).Sum
    if ($totalFailures -gt 0) {
        Write-Host ""
        Write-Host "${Red}=== Active Issues ===${Reset}"
        foreach ($service in $script:ServiceFailures.Keys) {
            if ($script:ServiceFailures[$service] -gt 0) {
                Write-Host "${Red}$service: $($script:ServiceFailures[$service]) consecutive failures${Reset}"
            }
        }
    }
    
    Write-Host ""
    Write-Host "${Yellow}Press Ctrl+C to stop monitoring${Reset}"
}

function Start-Monitoring {
    Write-Log "Starting BPTS service monitoring" "INFO" $Cyan
    Write-Log "Log file: $LogFile" "INFO" $Cyan
    Write-Log "Alert threshold: $AlertThreshold failures" "INFO" $Cyan
    Write-Log "Check interval: $CheckInterval seconds" "INFO" $Cyan
    
    $services = @(
        @{
            Name = "Web Application"
            Url = "http://localhost:3000"
        },
        @{
            Name = "API Service"
            Url = "http://localhost:5000/health"
        },
        @{
            Name = "SQL Server"
            Command = 'docker exec bpts-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT 1" 2>$null'
        },
        @{
            Name = "Redis"
            Command = "docker exec bpts-redis-1 redis-cli ping 2>$null"
        }
    )
    
    try {
        while ($true) {
            $results = @()
            
            foreach ($service in $services) {
                $result = Monitor-Service $service
                $result.ServiceName = $service.Name
                $results += $result
            }
            
            # Get system metrics
            $systemMetrics = Get-SystemMetrics
            
            # Show dashboard
            Show-MonitoringStatus $results $systemMetrics
            
            # Wait for next check
            Start-Sleep -Seconds $CheckInterval
        }
    }
    catch {
        Write-Log "Monitoring stopped: $_" "INFO" $Yellow
    }
}

# Trap Ctrl+C for graceful shutdown
$null = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    Write-Log "Monitoring service shutting down gracefully" "INFO" $Yellow
}

# Start monitoring
Start-Monitoring
