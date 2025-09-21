#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Docker cleanup script for BPTS Work Intake System
.DESCRIPTION
    This script provides various cleanup operations for Docker resources
.PARAMETER CleanContainers
    Remove stopped containers
.PARAMETER CleanImages
    Remove unused images
.PARAMETER CleanVolumes
    Remove unused volumes (WARNING: Data loss!)
.PARAMETER CleanNetworks
    Remove unused networks
.PARAMETER CleanAll
    Perform complete cleanup (WARNING: Data loss!)
.PARAMETER Force
    Force cleanup without confirmation prompts
.PARAMETER DryRun
    Show what would be cleaned without actually doing it
.EXAMPLE
    .\Cleanup-Docker.ps1 -CleanContainers -CleanImages
.EXAMPLE
    .\Cleanup-Docker.ps1 -CleanAll -Force
.EXAMPLE
    .\Cleanup-Docker.ps1 -DryRun
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$CleanContainers,
    
    [Parameter(Mandatory = $false)]
    [switch]$CleanImages,
    
    [Parameter(Mandatory = $false)]
    [switch]$CleanVolumes,
    
    [Parameter(Mandatory = $false)]
    [switch]$CleanNetworks,
    
    [Parameter(Mandatory = $false)]
    [switch]$CleanAll,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force,
    
    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

# Script configuration
$ErrorActionPreference = "Continue"

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

function Confirm-Action {
    param([string]$Message, [string]$WarningMessage = $null)
    
    if ($Force) {
        return $true
    }
    
    if ($WarningMessage) {
        Write-Warning $WarningMessage
    }
    
    $response = Read-Host "$Message (y/N)"
    return $response -match '^[Yy]'
}

function Get-DockerResourceUsage {
    Write-Info "Getting Docker resource usage..."
    
    try {
        $systemInfo = docker system df --format "table {{.Type}}\t{{.Total}}\t{{.Active}}\t{{.Size}}\t{{.Reclaimable}}"
        Write-Info "Current Docker resource usage:"
        $systemInfo | ForEach-Object { Write-Host "  $_" }
        Write-Host ""
        
        return $systemInfo
    }
    catch {
        Write-Warning "Could not get Docker resource usage: $_"
        return $null
    }
}

function Stop-BPTSContainers {
    Write-Info "Stopping BPTS containers..."
    
    if ($DryRun) {
        Write-Info "[DRY RUN] Would stop BPTS containers"
        return
    }
    
    try {
        docker-compose down --remove-orphans 2>$null
        Write-Success "BPTS containers stopped"
    }
    catch {
        Write-Warning "Error stopping containers: $_"
    }
}

function Clean-StoppedContainers {
    Write-Info "Cleaning stopped containers..."
    
    try {
        # Get stopped containers
        $stoppedContainers = docker ps -a --filter "status=exited" --format "{{.ID}} {{.Names}} {{.Status}}"
        
        if (-not $stoppedContainers) {
            Write-Info "No stopped containers found"
            return
        }
        
        Write-Info "Found stopped containers:"
        $stoppedContainers | ForEach-Object { Write-Info "  $_" }
        
        if ($DryRun) {
            Write-Info "[DRY RUN] Would remove $($stoppedContainers.Count) stopped containers"
            return
        }
        
        if (Confirm-Action "Remove all stopped containers?") {
            docker container prune -f
            Write-Success "Stopped containers removed"
        }
    }
    catch {
        Write-Error "Failed to clean stopped containers: $_"
    }
}

function Clean-UnusedImages {
    Write-Info "Cleaning unused images..."
    
    try {
        # Get unused images
        $unusedImages = docker images --filter "dangling=true" --format "{{.ID}} {{.Repository}} {{.Tag}} {{.Size}}"
        
        if (-not $unusedImages) {
            Write-Info "No unused images found"
            return
        }
        
        Write-Info "Found unused images:"
        $unusedImages | ForEach-Object { Write-Info "  $_" }
        
        if ($DryRun) {
            Write-Info "[DRY RUN] Would remove unused images"
            return
        }
        
        if (Confirm-Action "Remove unused images?") {
            docker image prune -f
            Write-Success "Unused images removed"
            
            # Optionally remove all unused images (not just dangling)
            if (Confirm-Action "Also remove all unused images (not just dangling)?") {
                docker image prune -a -f
                Write-Success "All unused images removed"
            }
        }
    }
    catch {
        Write-Error "Failed to clean unused images: $_"
    }
}

function Clean-UnusedVolumes {
    Write-Warning "Volume cleanup will permanently delete data!"
    Write-Info "Cleaning unused volumes..."
    
    try {
        # Get unused volumes
        $unusedVolumes = docker volume ls --filter "dangling=true" --format "{{.Name}} {{.Driver}}"
        
        if (-not $unusedVolumes) {
            Write-Info "No unused volumes found"
            return
        }
        
        Write-Info "Found unused volumes:"
        $unusedVolumes | ForEach-Object { Write-Info "  $_" }
        
        if ($DryRun) {
            Write-Info "[DRY RUN] Would remove unused volumes"
            return
        }
        
        if (Confirm-Action "Remove unused volumes?" "This will permanently delete data in these volumes!") {
            docker volume prune -f
            Write-Success "Unused volumes removed"
        }
    }
    catch {
        Write-Error "Failed to clean unused volumes: $_"
    }
}

function Clean-UnusedNetworks {
    Write-Info "Cleaning unused networks..."
    
    try {
        # Get unused networks
        $unusedNetworks = docker network ls --filter "dangling=true" --format "{{.ID}} {{.Name}} {{.Driver}}"
        
        if (-not $unusedNetworks) {
            Write-Info "No unused networks found"
            return
        }
        
        Write-Info "Found unused networks:"
        $unusedNetworks | ForEach-Object { Write-Info "  $_" }
        
        if ($DryRun) {
            Write-Info "[DRY RUN] Would remove unused networks"
            return
        }
        
        if (Confirm-Action "Remove unused networks?") {
            docker network prune -f
            Write-Success "Unused networks removed"
        }
    }
    catch {
        Write-Error "Failed to clean unused networks: $_"
    }
}

function Clean-BuildCache {
    Write-Info "Cleaning build cache..."
    
    try {
        # Get build cache info
        $buildCache = docker system df --format "table {{.Type}}\t{{.Total}}\t{{.Active}}\t{{.Size}}\t{{.Reclaimable}}" | Select-String "Build Cache"
        
        if ($buildCache) {
            Write-Info "Found build cache: $buildCache"
        }
        
        if ($DryRun) {
            Write-Info "[DRY RUN] Would clean build cache"
            return
        }
        
        if (Confirm-Action "Clean build cache?") {
            docker builder prune -f
            Write-Success "Build cache cleaned"
        }
    }
    catch {
        Write-Error "Failed to clean build cache: $_"
    }
}

function Perform-SystemPrune {
    Write-Warning "System prune will remove all unused Docker resources!"
    
    if ($DryRun) {
        Write-Info "[DRY RUN] Would perform system prune"
        return
    }
    
    if (Confirm-Action "Perform complete system prune?" "This will remove all unused containers, networks, images, and build cache!") {
        try {
            docker system prune -f
            Write-Success "System prune completed"
            
            if (Confirm-Action "Also remove unused volumes?" "This will permanently delete data!") {
                docker system prune --volumes -f
                Write-Success "System prune with volumes completed"
            }
        }
        catch {
            Write-Error "System prune failed: $_"
        }
    }
}

function Show-CleanupSummary {
    Write-Info "=== Cleanup Summary ==="
    
    # Show space reclaimed
    $afterUsage = Get-DockerResourceUsage
    
    Write-Info "Cleanup operations completed"
    Write-Info "Run 'docker system df' to see current resource usage"
}

function Start-Cleanup {
    Write-Info "Starting Docker cleanup for BPTS Work Intake System"
    
    if ($DryRun) {
        Write-Warning "DRY RUN MODE - No actual changes will be made"
    }
    
    Write-Info ""
    
    # Show current usage
    Get-DockerResourceUsage
    
    # Set cleanup flags
    if ($CleanAll) {
        $CleanContainers = $true
        $CleanImages = $true
        $CleanNetworks = $true
        $CleanVolumes = $true
    }
    
    # If no specific cleanup options provided, ask user
    if (-not ($CleanContainers -or $CleanImages -or $CleanVolumes -or $CleanNetworks -or $CleanAll)) {
        Write-Info "No specific cleanup options provided. What would you like to clean?"
        Write-Info "1. Stopped containers"
        Write-Info "2. Unused images"
        Write-Info "3. Unused networks"
        Write-Info "4. Unused volumes (WARNING: Data loss!)"
        Write-Info "5. Build cache"
        Write-Info "6. Everything (System prune)"
        Write-Info "7. Stop BPTS containers only"
        
        $choice = Read-Host "Enter your choice (1-7, or comma-separated for multiple)"
        
        switch -Regex ($choice) {
            "1" { $CleanContainers = $true }
            "2" { $CleanImages = $true }
            "3" { $CleanNetworks = $true }
            "4" { $CleanVolumes = $true }
            "5" { Clean-BuildCache }
            "6" { Perform-SystemPrune; return }
            "7" { Stop-BPTSContainers; return }
        }
    }
    
    # Stop BPTS containers first
    if ($CleanContainers -or $CleanVolumes -or $CleanAll) {
        Stop-BPTSContainers
    }
    
    # Perform cleanup operations
    if ($CleanContainers) {
        Clean-StoppedContainers
    }
    
    if ($CleanImages) {
        Clean-UnusedImages
    }
    
    if ($CleanNetworks) {
        Clean-UnusedNetworks
    }
    
    if ($CleanVolumes) {
        Clean-UnusedVolumes
    }
    
    # Clean build cache
    Clean-BuildCache
    
    # Show summary
    Show-CleanupSummary
    
    Write-Success "Docker cleanup completed!"
}

# Main execution
Start-Cleanup
