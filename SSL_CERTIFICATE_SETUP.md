# SSL Certificate Setup Guide

## Overview
This guide provides step-by-step instructions for configuring SSL certificates for the Work Intake System in production.

## Prerequisites
- Windows Server with IIS installed
- Domain name configured
- Administrative access to the server

## Option 1: Using Let's Encrypt (Free)

### 1. Install Certbot for Windows
```powershell
# Download and install Certbot
Invoke-WebRequest -Uri "https://dl.eff.org/certbot-beta-installer-win32.exe" -OutFile "certbot-installer.exe"
.\certbot-installer.exe
```

### 2. Generate Certificate
```powershell
# Generate certificate for your domain
certbot certonly --webroot -w C:\inetpub\wwwroot\WorkIntakeSystem -d workintake.yourcompany.com
```

### 3. Configure IIS
```powershell
# Import certificate to IIS
Import-Certificate -FilePath "C:\Certbot\live\workintake.yourcompany.com\fullchain.pem" -CertStoreLocation Cert:\LocalMachine\My
```

### 4. Bind Certificate to Website
```powershell
# Create HTTPS binding
New-WebBinding -Name "WorkIntakeSystem" -Protocol "https" -Port 443 -SslFlags 1
```

## Option 2: Using Commercial Certificate

### 1. Generate CSR (Certificate Signing Request)
```powershell
# Generate private key and CSR
openssl req -new -newkey rsa:2048 -nodes -keyout workintake.key -out workintake.csr
```

### 2. Submit CSR to Certificate Authority
- Copy the contents of `workintake.csr`
- Submit to your chosen CA (DigiCert, GlobalSign, etc.)
- Download the issued certificate

### 3. Install Certificate
```powershell
# Import certificate
Import-Certificate -FilePath "workintake.crt" -CertStoreLocation Cert:\LocalMachine\My
```

## Configuration Files

### appsettings.Production.json
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "certificates/workintake.pfx",
          "Password": "your-certificate-password"
        }
      }
    }
  }
}
```

### web.config (IIS)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="30000000" />
      </requestFiltering>
    </security>
    <httpProtocol>
      <customHeaders>
        <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-XSS-Protection" value="1; mode=block" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

## SSL Certificate Renewal

### Let's Encrypt Auto-Renewal
```powershell
# Create scheduled task for auto-renewal
$action = New-ScheduledTaskAction -Execute "certbot" -Argument "renew --quiet"
$trigger = New-ScheduledTaskTrigger -Daily -At 2AM
Register-ScheduledTask -TaskName "Certbot Renewal" -Action $action -Trigger $trigger
```

### Commercial Certificate Renewal
- Monitor certificate expiration dates
- Generate new CSR when needed
- Follow CA's renewal process
- Update certificate in IIS

## Security Best Practices

### 1. Strong Cipher Suites
```powershell
# Configure strong cipher suites in IIS
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Ciphers\DES 56/56" -Name "Enabled" -Value 0
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Ciphers\RC2 40/128" -Name "Enabled" -Value 0
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Ciphers\RC2 56/128" -Name "Enabled" -Value 0
```

### 2. HTTP to HTTPS Redirect
```csharp
// In Program.cs
if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

### 3. Security Headers
```csharp
// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

## Troubleshooting

### Common Issues

1. **Certificate Not Trusted**
   - Ensure certificate chain is complete
   - Check intermediate certificates
   - Verify certificate is in correct store

2. **SSL Handshake Failures**
   - Check cipher suite compatibility
   - Verify certificate matches domain
   - Check firewall settings

3. **Mixed Content Errors**
   - Ensure all resources use HTTPS
   - Update application URLs
   - Check third-party integrations

### Verification Commands
```powershell
# Test SSL configuration
Test-NetConnection -ComputerName workintake.yourcompany.com -Port 443

# Check certificate details
Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*workintake*"}

# Verify IIS binding
Get-WebBinding -Name "WorkIntakeSystem"
```

## Monitoring

### Certificate Expiration Monitoring
```powershell
# Script to check certificate expiration
$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*workintake*"}
$daysUntilExpiry = ($cert.NotAfter - (Get-Date)).Days

if ($daysUntilExpiry -lt 30) {
    Write-Warning "Certificate expires in $daysUntilExpiry days"
    # Send notification email
}
```

### SSL Labs Testing
- Use SSL Labs (https://www.ssllabs.com/ssltest/) to test SSL configuration
- Aim for A+ rating
- Address any security issues identified

## Conclusion
Proper SSL certificate configuration is essential for production security. Follow this guide to ensure your Work Intake System is properly secured with HTTPS. 