#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Data backup script for BPTS Work Intake System
.DESCRIPTION
    This script creates backups of database and application data from Docker containers
.PARAMETER BackupPath
    Path where backups will be stored (default: backups/)
.PARAMETER BackupType
    Type of backup: Database, Files, All (default: All)
.PARAMETER Compress
    Compress backup files
.PARAMETER Retention
    Number of backup files to retain (default: 7)
.PARAMETER DatabaseOnly
    Backup only the database
.PARAMETER FilesOnly
    Backup only application files
.EXAMPLE
    .\Backup-Data.ps1
.EXAMPLE
    .\Backup-Data.ps1 -BackupPath "D:\Backups" -Compress -Retention 14
.EXAMPLE
    .\Backup-Data.ps1 -DatabaseOnly
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$BackupPath = "backups",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("Database", "Files", "All")]
    [string]$BackupType = "All",
    
    [Parameter(Mandatory = $false)]
    [switch]$Compress,
    
    [Parameter(Mandatory = $false)]
    [int]$Retention = 7,
    
    [Parameter(Mandatory = $false)]
    [switch]$DatabaseOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$FilesOnly
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

function Initialize-BackupDirectory {
    param([string]$Path)
    
    try {
        if (-not (Test-Path $Path)) {
            New-Item -ItemType Directory -Path $Path -Force | Out-Null
            Write-Info "Created backup directory: $Path"
        }
        
        # Create subdirectories
        $subdirs = @("database", "files", "logs")
        foreach ($subdir in $subdirs) {
            $fullPath = Join-Path $Path $subdir
            if (-not (Test-Path $fullPath)) {
                New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
            }
        }
        
        return $true
    }
    catch {
        Write-Error "Failed to initialize backup directory: $_"
        return $false
    }
}

function Test-ContainerRunning {
    param([string]$ContainerName)
    
    try {
        $result = docker inspect $ContainerName --format "{{.State.Running}}" 2>$null
        return $result -eq "true"
    }
    catch {
        return $false
    }
}

function Backup-Database {
    param([string]$BackupDir)
    
    Write-Info "Starting database backup..."
    
    $containerName = "bpts-sqlserver-1"
    if (-not (Test-ContainerRunning $containerName)) {
        Write-Error "SQL Server container is not running"
        return $false
    }
    
    try {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupFileName = "WorkIntakeSystem_$timestamp.bak"
        $backupPath = "/var/opt/mssql/backup/$backupFileName"
        $localBackupPath = Join-Path $BackupDir "database\$backupFileName"
        
        # Create backup directory in container
        docker exec $containerName mkdir -p /var/opt/mssql/backup
        
        # Create database backup
        $backupQuery = @"
BACKUP DATABASE [WorkIntakeSystem] 
TO DISK = '$backupPath'
WITH FORMAT, COMPRESSION, CHECKSUM, 
NAME = 'WorkIntakeSystem Full Backup $timestamp'
"@
        
        Write-Info "Creating database backup in container..."
        docker exec $containerName /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q $backupQuery
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Database backup failed"
            return $false
        }
        
        # Copy backup file from container to host
        Write-Info "Copying backup file to host..."
        docker cp "${containerName}:${backupPath}" $localBackupPath
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to copy backup file from container"
            return $false
        }
        
        # Verify backup file
        if (Test-Path $localBackupPath) {
            $fileSize = (Get-Item $localBackupPath).Length
            Write-Success "Database backup completed: $localBackupPath ($([math]::Round($fileSize / 1MB, 2)) MB)"
            
            # Clean up backup file in container
            docker exec $containerName rm -f $backupPath
            
            return $localBackupPath
        } else {
            Write-Error "Backup file not found after copy"
            return $false
        }
    }
    catch {
        Write-Error "Database backup failed: $_"
        return $false
    }
}

function Backup-ApplicationFiles {
    param([string]$BackupDir)
    
    Write-Info "Starting application files backup..."
    
    try {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupFileName = "AppFiles_$timestamp"
        $backupPath = Join-Path $BackupDir "files\$backupFileName"
        
        # Create backup directory
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
        
        # Backup application logs
        $logContainers = @("bpts-workintake-api-1", "bpts-workintake-web-1")
        foreach ($container in $logContainers) {
            if (Test-ContainerRunning $container) {
                Write-Info "Backing up logs from $container..."
                
                $logDir = Join-Path $backupPath "logs\$container"
                New-Item -ItemType Directory -Path $logDir -Force | Out-Null
                
                # Copy logs if they exist
                try {
                    docker cp "${container}:/app/logs" $logDir 2>$null
                }
                catch {
                    Write-Warning "No logs found in $container or failed to copy"
                }
            }
        }
        
        # Backup configuration files
        Write-Info "Backing up configuration files..."
        $configDir = Join-Path $backupPath "config"
        New-Item -ItemType Directory -Path $configDir -Force | Out-Null
        
        # Copy important config files
        $configFiles = @(
            "docker-compose.yml",
            "docker-compose.override.yml",
            "appsettings.json",
            "appsettings.Production.json"
        )
        
        foreach ($configFile in $configFiles) {
            if (Test-Path $configFile) {
                Copy-Item $configFile $configDir -Force
            }
        }
        
        Write-Success "Application files backup completed: $backupPath"
        return $backupPath
    }
    catch {
        Write-Error "Application files backup failed: $_"
        return $false
    }
}

function Compress-Backup {
    param([string]$BackupPath, [string]$OutputPath)
    
    Write-Info "Compressing backup..."
    
    try {
        if (Get-Command "7z" -ErrorAction SilentlyContinue) {
            # Use 7-Zip if available
            $compressedFile = "$OutputPath.7z"
            & 7z a -t7z -mx9 $compressedFile $BackupPath
            
            if ($LASTEXITCODE -eq 0) {
                Remove-Item $BackupPath -Recurse -Force
                Write-Success "Backup compressed: $compressedFile"
                return $compressedFile
            }
        }
        elseif (Get-Command "Compress-Archive" -ErrorAction SilentlyContinue) {
            # Use PowerShell compression
            $compressedFile = "$OutputPath.zip"
            Compress-Archive -Path $BackupPath -DestinationPath $compressedFile -Force
            
            Remove-Item $BackupPath -Recurse -Force
            Write-Success "Backup compressed: $compressedFile"
            return $compressedFile
        }
        else {
            Write-Warning "No compression tool available, keeping uncompressed backup"
            return $BackupPath
        }
    }
    catch {
        Write-Warning "Compression failed, keeping uncompressed backup: $_"
        return $BackupPath
    }
}

function Clean-OldBackups {
    param([string]$BackupDir, [int]$RetentionDays)
    
    Write-Info "Cleaning old backups (retention: $RetentionDays days)..."
    
    try {
        $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
        
        # Clean database backups
        $dbBackupDir = Join-Path $BackupDir "database"
        if (Test-Path $dbBackupDir) {
            $oldDbBackups = Get-ChildItem $dbBackupDir -File | Where-Object { $_.CreationTime -lt $cutoffDate }
            foreach ($backup in $oldDbBackups) {
                Remove-Item $backup.FullName -Force
                Write-Info "Removed old database backup: $($backup.Name)"
            }
        }
        
        # Clean file backups
        $fileBackupDir = Join-Path $BackupDir "files"
        if (Test-Path $fileBackupDir) {
            $oldFileBackups = Get-ChildItem $fileBackupDir -Directory | Where-Object { $_.CreationTime -lt $cutoffDate }
            foreach ($backup in $oldFileBackups) {
                Remove-Item $backup.FullName -Recurse -Force
                Write-Info "Removed old file backup: $($backup.Name)"
            }
            
            # Also clean compressed file backups
            $oldCompressedBackups = Get-ChildItem $fileBackupDir -File | Where-Object { $_.CreationTime -lt $cutoffDate -and ($_.Extension -eq ".zip" -or $_.Extension -eq ".7z") }
            foreach ($backup in $oldCompressedBackups) {
                Remove-Item $backup.FullName -Force
                Write-Info "Removed old compressed backup: $($backup.Name)"
            }
        }
        
        Write-Success "Old backup cleanup completed"
    }
    catch {
        Write-Warning "Failed to clean old backups: $_"
    }
}

function Start-Backup {
    Write-Info "Starting BPTS backup process..."
    Write-Info "Backup type: $BackupType"
    Write-Info "Backup path: $BackupPath"
    Write-Info "Compression: $Compress"
    Write-Info "Retention: $Retention days"
    Write-Info ""
    
    # Override BackupType based on switches
    if ($DatabaseOnly) { $BackupType = "Database" }
    if ($FilesOnly) { $BackupType = "Files" }
    
    # Initialize backup directory
    if (-not (Initialize-BackupDirectory $BackupPath)) {
        exit 1
    }
    
    $backupResults = @()
    
    try {
        # Backup database
        if ($BackupType -eq "Database" -or $BackupType -eq "All") {
            $dbBackup = Backup-Database $BackupPath
            if ($dbBackup) {
                $backupResults += @{ Type = "Database"; Path = $dbBackup; Success = $true }
            } else {
                $backupResults += @{ Type = "Database"; Path = $null; Success = $false }
            }
        }
        
        # Backup application files
        if ($BackupType -eq "Files" -or $BackupType -eq "All") {
            $fileBackup = Backup-ApplicationFiles $BackupPath
            if ($fileBackup) {
                # Compress if requested
                if ($Compress) {
                    $compressedBackup = Compress-Backup $fileBackup (Join-Path (Split-Path $fileBackup) (Split-Path $fileBackup -Leaf))
                    $backupResults += @{ Type = "Files"; Path = $compressedBackup; Success = $true }
                } else {
                    $backupResults += @{ Type = "Files"; Path = $fileBackup; Success = $true }
                }
            } else {
                $backupResults += @{ Type = "Files"; Path = $null; Success = $false }
            }
        }
        
        # Clean old backups
        Clean-OldBackups $BackupPath $Retention
        
        # Show summary
        Write-Info ""
        Write-Info "=== Backup Summary ==="
        foreach ($result in $backupResults) {
            if ($result.Success) {
                Write-Success "$($result.Type) backup: $($result.Path)"
            } else {
                Write-Error "$($result.Type) backup failed"
            }
        }
        
        $successCount = ($backupResults | Where-Object { $_.Success }).Count
        $totalCount = $backupResults.Count
        
        if ($successCount -eq $totalCount) {
            Write-Success "All backups completed successfully!"
        } else {
            Write-Warning "$successCount of $totalCount backups completed successfully"
        }
        
    }
    catch {
        Write-Error "Backup process failed: $_"
        exit 1
    }
}

# Main execution
Start-Backup
