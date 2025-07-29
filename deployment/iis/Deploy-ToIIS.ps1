# Work Intake System IIS Deployment Script
# This script deploys the Work Intake System to IIS with JWT authentication

param(
    [string]$SiteName = "WorkIntakeSystem",
    [string]$AppPoolName = "WorkIntakeSystem",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\WorkIntakeSystem",
    [int]$Port = 80,
    [int]$HttpsPort = 443,
    [bool]$EnableSSL = $false,
    [string]$CertificateThumbprint = "",
    [string]$Environment = "Production"
)

# Ensure running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

try {
    Write-Host "Starting Work Intake System deployment..." -ForegroundColor Green
    
    # 1. Create Application Pool
    Write-Host "Creating Application Pool: $AppPoolName" -ForegroundColor Yellow
    
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Remove-WebAppPool -Name $AppPoolName
        Write-Host "Removed existing Application Pool: $AppPoolName" -ForegroundColor Yellow
    }
    
    New-WebAppPool -Name $AppPoolName -Force
    
    # Configure Application Pool settings
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value ApplicationPoolIdentity
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name recycling.periodicRestart.time -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name startMode -Value AlwaysRunning
    
    # 2. Create Physical Directory
    Write-Host "Creating Physical Directory: $PhysicalPath" -ForegroundColor Yellow
    
    if (!(Test-Path $PhysicalPath)) {
        New-Item -ItemType Directory -Path $PhysicalPath -Force
    }
    
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
    
    # 5. Configure JWT Authentication (Anonymous enabled for JWT)
    Write-Host "Configuring JWT Authentication..." -ForegroundColor Yellow
    Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" -Name enabled -Value $false -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/anonymousAuthentication" -Name enabled -Value $true -PSPath "IIS:\" -Location "$SiteName"
    
    # 6. Configure Application Settings
    Write-Host "Configuring application settings..." -ForegroundColor Yellow
    
    # Set environment variable
    Set-WebConfigurationProperty -Filter "/system.webServer/aspNetCore/environmentVariables/environmentVariable[@name='ASPNETCORE_ENVIRONMENT']" -Name value -Value $Environment -PSPath "IIS:\" -Location "$SiteName"
    
    # 7. Configure URL Rewrite for SPA
    Write-Host "Configuring URL Rewrite for SPA..." -ForegroundColor Yellow
    
    $rewriteRule = @"
    <rule name="SPA Routes" stopProcessing="true">
        <match url=".*" />
        <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/(api)" negate="true" />
        </conditions>
        <action type="Rewrite" url="/" />
    </rule>
"@
    
    Add-WebConfigurationProperty -Filter "/system.webServer/rewrite/rules" -Name "." -Value @{name="SPA Routes"} -PSPath "IIS:\" -Location "$SiteName"
    
    # 8. Configure Security Headers
    Write-Host "Configuring security headers..." -ForegroundColor Yellow
    
    $headers = @(
        @{name="X-Frame-Options"; value="SAMEORIGIN"},
        @{name="X-XSS-Protection"; value="1; mode=block"},
        @{name="X-Content-Type-Options"; value="nosniff"},
        @{name="Referrer-Policy"; value="strict-origin-when-cross-origin"}
    )
    
    foreach ($header in $headers) {
        Add-WebConfigurationProperty -Filter "/system.webServer/httpProtocol/customHeaders" -Name "." -Value $header -PSPath "IIS:\" -Location "$SiteName"
    }
    
    # 9. Configure Compression
    Write-Host "Configuring compression..." -ForegroundColor Yellow
    
    Enable-WebGlobalModule -Name "DynamicCompressionModule"
    Enable-WebGlobalModule -Name "StaticCompressionModule"
    
    # 10. Configure Caching
    Write-Host "Configuring caching..." -ForegroundColor Yellow
    
    $cacheExtensions = @(".css", ".js", ".png", ".jpg", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot")
    
    foreach ($ext in $cacheExtensions) {
        Add-WebConfigurationProperty -Filter "/system.webServer/caching/profiles" -Name "." -Value @{extension=$ext; policy="CacheForTimePeriod"; duration="00:01:00:00"} -PSPath "IIS:\" -Location "$SiteName"
    }
    
    # 11. Set File Permissions
    Write-Host "Setting file permissions..." -ForegroundColor Yellow
    
    $acl = Get-Acl $PhysicalPath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$AppPoolName", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    Set-Acl -Path $PhysicalPath -AclObject $acl
    
    # 12. Start the website
    Write-Host "Starting website..." -ForegroundColor Yellow
    Start-Website -Name $SiteName
    
    # 13. Health Check
    Write-Host "Performing health check..." -ForegroundColor Yellow
    
    $url = if ($EnableSSL -and $CertificateThumbprint) { "https://localhost:$HttpsPort" } else { "http://localhost:$Port" }
    
    try {
        $response = Invoke-WebRequest -Uri "$url/health" -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "Health check passed!" -ForegroundColor Green
        } else {
            Write-Warning "Health check returned status code: $($response.StatusCode)"
        }
    } catch {
        Write-Warning "Health check failed: $($_.Exception.Message)"
    }
    
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
    Write-Host "Website URL: $url" -ForegroundColor Cyan
    Write-Host "Application Pool: $AppPoolName" -ForegroundColor Cyan
    Write-Host "Physical Path: $PhysicalPath" -ForegroundColor Cyan
    
} catch {
    Write-Error "Deployment failed: $($_.Exception.Message)"
    exit 1
} 