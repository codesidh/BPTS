# Work Intake System - Deployment Guide

## Overview
This guide covers the complete deployment process for the Work Intake System, including authentication setup, email configuration, SSL certificates, and production security.

## Prerequisites
- .NET 8.0 SDK
- SQL Server (or SQL Server Express)
- Redis Server
- IIS (for production deployment)
- SSL Certificate

## 1. Authentication Flow Testing

### Test User Registration and Login
1. Start the application:
   ```bash
   cd src/WorkIntakeSystem.API
   dotnet run
   ```

2. Open the frontend application:
   ```bash
   cd src/WorkIntakeSystem.Web
   npm install
   npm run dev
   ```

3. Test the authentication flow:
   - Register a new user at `http://localhost:3000/register`
   - Login with the registered user at `http://localhost:3000/login`
   - Test logout functionality
   - Verify JWT token is stored in localStorage

### Run Authentication Tests
```bash
cd src/WorkIntakeSystem.Tests
dotnet test --filter "FullyQualifiedName~AuthenticationTests"
```

## 2. Email Service Configuration

### Configure SMTP Settings
Update `src/WorkIntakeSystem.API/appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "noreply@workintakesystem.com",
    "FromName": "Work Intake System",
    "AppUrl": "https://localhost:3000"
  }
}
```

### Gmail Setup (Example)
1. Enable 2-factor authentication on your Gmail account
2. Generate an App Password:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate a new app password for "Mail"
3. Use the generated password in the SmtpPassword field

### Test Email Functionality
1. Test password reset:
   - Go to login page
   - Click "Forgot Password"
   - Enter your email
   - Check email for reset link
   - Test the reset password flow

## 3. SSL Certificate Setup

### Development SSL Certificate
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Production SSL Certificate
1. Obtain an SSL certificate from a trusted CA (Let's Encrypt, DigiCert, etc.)
2. For IIS deployment:
   - Install the certificate in IIS
   - Bind it to your website
   - Configure HTTPS redirect

3. For Kestrel deployment, update `appsettings.Production.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "certificates/your-certificate.pfx",
          "Password": "your-certificate-password"
        }
      }
    }
  }
}
```

## 4. User Roles and Permissions Configuration

### Default Role Hierarchy
- **SystemAdministrator**: Full access to all features
- **BusinessExecutive**: Strategic oversight and approval
- **Director**: Department-level management
- **Manager**: Team management and approval
- **Lead**: Team leadership
- **EndUser**: Basic work request creation and voting

### Assign Initial Administrator
1. Register a user through the application
2. Use the database to assign SystemAdministrator role:
```sql
UPDATE Users 
SET Role = 6 
WHERE Email = 'admin@yourcompany.com'
```

### Test Role-Based Access
1. Login as different users with different roles
2. Verify access to different features based on permissions
3. Test the role management endpoints:
   - `POST /api/role/assign`
   - `POST /api/role/remove`
   - `GET /api/role/permissions/{userId}`

## 5. Protected Endpoints Testing

### Test All Protected Endpoints
Use the following test script or Swagger UI:

```bash
# Test with authentication
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5000/api/auth/me

# Test without authentication (should fail)
curl http://localhost:5000/api/auth/me

# Test role-based endpoints
curl -H "Authorization: Bearer ADMIN_TOKEN" \
     http://localhost:5000/api/role/available-roles

curl -H "Authorization: Bearer USER_TOKEN" \
     http://localhost:5000/api/role/available-roles
```

### Endpoint Security Checklist
- [ ] `/api/auth/me` - Requires authentication
- [ ] `/api/workrequests` - Requires authentication
- [ ] `/api/users` - Requires user.read permission
- [ ] `/api/role/assign` - Requires user.assignrole permission
- [ ] `/api/analytics` - Requires analytics.view permission

## 6. Production Deployment

### Database Setup
1. Create production database:
```sql
CREATE DATABASE WorkIntakeSystemDb;
```

2. Run migrations:
```bash
cd src/WorkIntakeSystem.API
dotnet ef database update
```

### Environment Configuration
1. Update `appsettings.Production.json` with production values
2. Set environment variables:
```bash
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://localhost:5001
```

### IIS Deployment
1. Publish the application:
```bash
dotnet publish -c Release -o ./publish
```

2. Configure IIS:
   - Create new website
   - Set physical path to publish folder
   - Configure application pool (.NET Core)
   - Set up SSL binding

### Docker Deployment
```bash
docker build -t workintakesystem .
docker run -p 5001:5001 workintakesystem
```

## 7. Security Hardening

### JWT Configuration
- Use a strong, unique secret key (at least 64 characters)
- Set appropriate token expiration (8 hours for production)
- Enable token validation

### Rate Limiting
- Configure API Gateway rate limiting
- Set appropriate limits for production traffic

### CORS Configuration
- Restrict CORS to specific domains in production
- Remove wildcard origins

### Security Headers
- HSTS enabled
- Content Security Policy configured
- X-Frame-Options set to DENY
- X-Content-Type-Options set to nosniff

## 8. Monitoring and Logging

### Application Insights (Optional)
1. Add Application Insights package
2. Configure connection string in appsettings.Production.json
3. Monitor application performance and errors

### Logging Configuration
- Configure Serilog for file and console logging
- Set appropriate log levels for production
- Implement log rotation

## 9. Backup and Recovery

### Database Backup
```sql
BACKUP DATABASE WorkIntakeSystemDb 
TO DISK = 'C:\Backups\WorkIntakeSystemDb.bak'
WITH FORMAT, INIT, NAME = 'WorkIntakeSystemDb-Full Database Backup'
```

### Application Backup
- Backup configuration files
- Backup SSL certificates
- Document deployment configuration

## 10. Testing Checklist

### Authentication Tests
- [ ] User registration works
- [ ] User login works
- [ ] JWT token validation works
- [ ] Password reset flow works
- [ ] Email notifications are sent
- [ ] Logout clears tokens

### Authorization Tests
- [ ] Role-based access control works
- [ ] Permission-based access control works
- [ ] Unauthorized access is blocked
- [ ] Admin can assign roles
- [ ] Users can only access permitted features

### Security Tests
- [ ] HTTPS is enforced in production
- [ ] Security headers are present
- [ ] CORS is properly configured
- [ ] Rate limiting is working
- [ ] SQL injection protection is active

### Integration Tests
- [ ] All API endpoints respond correctly
- [ ] Database operations work
- [ ] Email service is functional
- [ ] Redis caching works
- [ ] External integrations are configured

## Troubleshooting

### Common Issues
1. **Email not sending**: Check SMTP configuration and credentials
2. **JWT token invalid**: Verify secret key and expiration settings
3. **Database connection failed**: Check connection string and firewall
4. **SSL certificate issues**: Verify certificate installation and binding

### Logs to Check
- Application logs: `logs/workintake-*.txt`
- IIS logs: `%SystemDrive%\inetpub\logs\LogFiles`
- Event Viewer: Application and System logs

## Support
For additional support or questions, refer to:
- API Documentation: Swagger UI at `/swagger`
- Test Results: `src/WorkIntakeSystem.Tests/TestResults/`
- Configuration: `appsettings.json` and `appsettings.Production.json` 