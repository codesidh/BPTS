# Work Intake System - IIS Deployment Script
# Enterprise deployment script for Windows Server with IIS

param(
    [Parameter(Mandatory = $true)]
    [string]$SiteName = "WorkIntakeSystem",
    
    [Parameter(Mandatory = $true)]
    [string]$PhysicalPath,
    
    [Parameter(Mandatory = $false)]
    [int]$Port = 80,
    
    [Parameter(Mandatory = $false)]
    [int]$HttpsPort = 443,
    
    [Parameter(Mandatory = $false)]
    [string]$AppPoolName = "WorkIntakeSystemPool",
    
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory = $false)]
    [string]$RedisConnectionString = "localhost:6379",
    
    [Parameter(Mandatory = $false)]
    [switch]$EnableSSL,
    
    [Parameter(Mandatory = $false)]
    [string]$CertificateThumbprint
)

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator. Exiting..."
    exit 1
}

Write-Host "Starting Work Intake System IIS Deployment..." -ForegroundColor Green

# Import required modules
Import-Module WebAdministration

try {
    # 1. Install IIS Features if not already installed
    Write-Host "Ensuring IIS features are installed..." -ForegroundColor Yellow
    
    $features = @(
        "IIS-WebServerRole",
        "IIS-WebServer",
        "IIS-CommonHttpFeatures",
        "IIS-ApplicationDevelopment",
        "IIS-NetFxExtensibility45",
        "IIS-ISAPIExtensions",
        "IIS-ISAPIFilter",
        "IIS-AspNetCoreModule",
        "IIS-ASPNET45",
        "IIS-WindowsAuthentication",
        "IIS-RequestFiltering",
        "IIS-StaticContent",
        "IIS-DefaultDocument",
        "IIS-DirectoryBrowsing",
        "IIS-HttpErrors",
        "IIS-HttpRedirect",
        "IIS-HttpCompressionStatic",
        "IIS-HttpCompressionDynamic",
        "IIS-ApplicationRequestRouting"
    )
    
    foreach ($feature in $features) {
        Enable-WindowsOptionalFeature -Online -FeatureName $feature -All -NoRestart
    }
    
    # 2. Create Application Pool
    Write-Host "Creating Application Pool: $AppPoolName" -ForegroundColor Yellow
    
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Remove-WebAppPool -Name $AppPoolName
        Write-Host "Removed existing Application Pool: $AppPoolName" -ForegroundColor Yellow
    }
    
    New-WebAppPool -Name $AppPoolName -Force
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value ApplicationPoolIdentity
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.loadUserProfile -Value $true
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name recycling.periodicRestart.time -Value "1.05:00:00"
    
    Write-Host "Application Pool created successfully" -ForegroundColor Green
    
    # 3. Create Website
    Write-Host "Creating Website: $SiteName" -ForegroundColor Yellow
    
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Remove-Website -Name $SiteName
        Write-Host "Removed existing Website: $SiteName" -ForegroundColor Yellow
    }
    
    New-Website -Name $SiteName -Port $Port -PhysicalPath $PhysicalPath -ApplicationPool $AppPoolName
    
    # 4. Configure HTTPS if enabled
    if ($EnableSSL -and $CertificateThumbprint) {
        Write-Host "Configuring HTTPS binding..." -ForegroundColor Yellow
        New-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort
        
        # Bind SSL certificate
        $cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Thumbprint -eq $CertificateThumbprint }
        if ($cert) {
            New-Item -Path "IIS:\SslBindings\0.0.0.0!$HttpsPort" -Value $cert -Force
            Write-Host "SSL certificate bound successfully" -ForegroundColor Green
        } else {
            Write-Warning "SSL certificate with thumbprint $CertificateThumbprint not found"
        }
    }
    
    # 5. Configure Windows Authentication
    Write-Host "Configuring Windows Authentication..." -ForegroundColor Yellow
    Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" -Name enabled -Value $true -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/anonymousAuthentication" -Name enabled -Value $false -PSPath "IIS:\" -Location "$SiteName"
    
    # 6. Configure Application Settings
    Write-Host "Configuring application settings..." -ForegroundColor Yellow
    
    if ($ConnectionString) {
        # Set connection string in web.config or appsettings
        # This would typically be done through secure configuration management
        Write-Host "Connection string configuration should be handled through secure config management" -ForegroundColor Yellow
    }
    
    # 7. Set Permissions
    Write-Host "Setting file permissions..." -ForegroundColor Yellow
    
    # Grant IIS_IUSRS read and execute permissions
    icacls $PhysicalPath /grant "IIS_IUSRS:(OI)(CI)RX" /T
    
    # Grant Application Pool identity modify permissions to logs folder
    $logsPath = Join-Path $PhysicalPath "logs"
    if (!(Test-Path $logsPath)) {
        New-Item -ItemType Directory -Path $logsPath -Force
    }
    icacls $logsPath /grant "IIS AppPool\$AppPoolName:(OI)(CI)M" /T
    
    # 8. Configure IIS Modules
    Write-Host "Configuring IIS modules..." -ForegroundColor Yellow
    
    # Enable compression
    Set-WebConfigurationProperty -Filter "/system.webServer/httpCompression" -Name directory -Value "%SystemRoot%\temp\IIS Temporary Compressed Files" -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "/system.webServer/httpCompression" -Name staticCompressionEnableCpuUsage -Value 80 -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "/system.webServer/httpCompression" -Name dynamicCompressionEnableCpuUsage -Value 80 -PSPath "IIS:\" -Location "$SiteName"
    
    # 9. Start Application Pool and Website
    Write-Host "Starting Application Pool and Website..." -ForegroundColor Yellow
    Start-WebAppPool -Name $AppPoolName
    Start-Website -Name $SiteName
    
    # 10. Verify deployment
    Write-Host "Verifying deployment..." -ForegroundColor Yellow
    
    $appPoolState = Get-WebAppPoolState -Name $AppPoolName
    $siteState = Get-WebsiteState -Name $SiteName
    
    Write-Host "Application Pool State: $($appPoolState.Value)" -ForegroundColor Cyan
    Write-Host "Website State: $($siteState.Value)" -ForegroundColor Cyan
    
    # Test HTTP response
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -UseBasicParsing -TimeoutSec 30
        Write-Host "Health check response: $($response.StatusCode)" -ForegroundColor Green
    } catch {
        Write-Warning "Health check failed: $($_.Exception.Message)"
    }
    
    Write-Host "`nDeployment completed successfully!" -ForegroundColor Green
    Write-Host "Site URL: http://localhost:$Port" -ForegroundColor Cyan
    if ($EnableSSL) {
        Write-Host "HTTPS URL: https://localhost:$HttpsPort" -ForegroundColor Cyan
    }
    
} catch {
    Write-Error "Deployment failed: $($_.Exception.Message)"
    Write-Error $_.ScriptStackTrace
    exit 1
}

# Display post-deployment checklist
Write-Host "`n=== POST-DEPLOYMENT CHECKLIST ===" -ForegroundColor Magenta
Write-Host "1. Verify database connection string is configured securely" -ForegroundColor White
Write-Host "2. Confirm Redis connection is working" -ForegroundColor White
Write-Host "3. Test Windows Authentication with domain users" -ForegroundColor White
Write-Host "4. Configure SSL certificate if using HTTPS" -ForegroundColor White  
Write-Host "5. Set up monitoring and logging" -ForegroundColor White
Write-Host "6. Configure firewall rules for external access" -ForegroundColor White
Write-Host "7. Test API endpoints and functionality" -ForegroundColor White
Write-Host "8. Set up automated backups" -ForegroundColor White 