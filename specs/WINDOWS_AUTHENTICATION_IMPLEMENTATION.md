# Windows Authentication Integration with JWT SSO Implementation Plan

## Overview
This document outlines the implementation plan for integrating Windows Authentication (NTLM/Kerberos) with the existing JWT authentication system, enabling Single Sign-On (SSO) while maintaining backward compatibility.

## Architecture Overview

### Current JWT Authentication Flow
1. User submits credentials via `/api/auth/login`
2. System validates credentials against local database
3. JWT token generated and returned
4. Client includes JWT in Authorization header for subsequent requests

### Enhanced Windows Authentication + JWT SSO Flow
1. **Windows Authentication Detection**: Middleware detects Windows authenticated user
2. **AD User Lookup**: System queries Active Directory for user information
3. **JWT Token Generation**: System generates JWT token for Windows authenticated user
4. **Hybrid Authentication**: System supports both Windows Auth and traditional JWT
5. **Fallback Mechanism**: Falls back to local authentication if Windows Auth fails

## Implementation Components

### 1. Windows Authentication Service

```csharp
// IWindowsAuthenticationService.cs
public interface IWindowsAuthenticationService
{
    Task<WindowsAuthResult> AuthenticateWindowsUserAsync(string windowsIdentity);
    Task<User?> GetOrCreateUserFromWindowsIdentityAsync(string windowsIdentity);
    Task<bool> SyncUserFromActiveDirectoryAsync(User user);
    Task<string> GenerateJwtTokenForWindowsUserAsync(User user);
    Task<WindowsAuthResult> ValidateWindowsAuthenticationAsync();
}

// WindowsAuthResult.cs
public class WindowsAuthResult
{
    public bool IsAuthenticated { get; set; }
    public string? WindowsIdentity { get; set; }
    public User? User { get; set; }
    public string? JwtToken { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### 2. Active Directory Integration Service

```csharp
// IActiveDirectoryService.cs
public interface IActiveDirectoryService
{
    Task<ADUserInfo?> GetUserFromActiveDirectoryAsync(string username);
    Task<List<string>> GetUserGroupsAsync(string username);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
    Task<ADUserInfo?> SearchUserByEmailAsync(string email);
}

// ADUserInfo.cs
public class ADUserInfo
{
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new List<string>();
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
}
```

### 3. Enhanced User Entity

```csharp
// User.cs - Additional properties
public class User : BaseEntity
{
    // Existing properties...
    
    [MaxLength(255)]
    public string? WindowsIdentity { get; set; }
    
    [MaxLength(255)]
    public string? ActiveDirectorySid { get; set; }
    
    public bool IsWindowsAuthenticated { get; set; } = false;
    
    public DateTime? LastAdSync { get; set; }
    
    public string? AdGroups { get; set; } // JSON field for AD groups
}
```

### 4. Windows Authentication Middleware

```csharp
// WindowsAuthenticationMiddleware.cs
public class WindowsAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWindowsAuthenticationService _windowsAuthService;

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if Windows Authentication is enabled
        if (context.User.Identity?.IsAuthenticated == true && 
            context.User.Identity.AuthenticationType == "NTLM" || 
            context.User.Identity.AuthenticationType == "Kerberos")
        {
            var windowsIdentity = context.User.Identity.Name;
            var authResult = await _windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);
            
            if (authResult.IsAuthenticated && authResult.JwtToken != null)
            {
                // Set JWT token in response header for client to capture
                context.Response.Headers.Add("X-Windows-Auth-Token", authResult.JwtToken);
            }
        }

        await _next(context);
    }
}
```

### 5. Enhanced Authentication Controller

```csharp
// AuthController.cs - Additional endpoints
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Existing endpoints...
    
    [HttpPost("windows-login")]
    public async Task<IActionResult> WindowsLogin()
    {
        var windowsIdentity = User.Identity?.Name;
        if (string.IsNullOrEmpty(windowsIdentity))
            return Unauthorized(new { message = "Windows authentication not available" });

        var authResult = await _windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);
        
        if (!authResult.IsAuthenticated)
            return Unauthorized(new { message = authResult.ErrorMessage });

        var response = new AuthenticationResponseDto
        {
            Token = authResult.JwtToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = _mapper.Map<UserDto>(authResult.User),
            AuthenticationType = "Windows"
        };

        return Ok(response);
    }

    [HttpPost("hybrid-login")]
    public async Task<IActionResult> HybridLogin([FromBody] LoginRequestDto request)
    {
        // Try Windows Authentication first
        var windowsIdentity = User.Identity?.Name;
        if (!string.IsNullOrEmpty(windowsIdentity))
        {
            var windowsAuthResult = await _windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);
            if (windowsAuthResult.IsAuthenticated)
            {
                return Ok(new AuthenticationResponseDto
                {
                    Token = windowsAuthResult.JwtToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = _mapper.Map<UserDto>(windowsAuthResult.User),
                    AuthenticationType = "Windows"
                });
            }
        }

        // Fallback to traditional JWT authentication
        var user = await _authService.ValidateUserAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = await _authService.GenerateJwtTokenAsync(user);
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(new AuthenticationResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = userDto,
            AuthenticationType = "JWT"
        });
    }
}
```

## Configuration Updates

### 1. appsettings.json

```json
{
  "Authentication": {
    "Windows": {
      "Enabled": true,
      "Domain": "your-domain.com",
      "LdapServer": "ldap://your-domain.com",
      "LdapPort": 389,
      "LdapUsername": "service-account@your-domain.com",
      "LdapPassword": "service-account-password",
      "SearchBase": "DC=your-domain,DC=com",
      "GroupMapping": {
        "IT-Admins": "Administrator",
        "Department-Heads": "Manager",
        "End-Users": "EndUser"
      },
      "SyncIntervalMinutes": 60,
      "AutoCreateUsers": true
    },
    "JwtSettings": {
      "Secret": "YourSuperSecretKeyHereThatIsAtLeast32CharactersLong",
      "Issuer": "WorkIntakeSystem",
      "Audience": "WorkIntakeSystem",
      "ExpirationHours": 24
    }
  }
}
```

### 2. Program.cs Updates

```csharp
// Add Windows Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Windows";
    options.DefaultChallengeScheme = "Windows";
})
.AddJwtBearer("JWT", options =>
{
    // Existing JWT configuration
})
.AddNegotiate("Windows", options =>
{
    options.EnableLdap = true;
    options.Domain = builder.Configuration["Authentication:Windows:Domain"];
});

// Add Windows Authentication services
builder.Services.AddScoped<IWindowsAuthenticationService, WindowsAuthenticationService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

// Add Windows Authentication middleware
app.UseMiddleware<WindowsAuthenticationMiddleware>();
```

## Database Migration

### 1. User Entity Updates

```csharp
// Migration: AddWindowsAuthenticationFields.cs
public partial class AddWindowsAuthenticationFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "WindowsIdentity",
            table: "Users",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ActiveDirectorySid",
            table: "Users",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsWindowsAuthenticated",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastAdSync",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AdGroups",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);

        // Add indexes for performance
        migrationBuilder.CreateIndex(
            name: "IX_Users_WindowsIdentity",
            table: "Users",
            column: "WindowsIdentity",
            unique: true,
            filter: "[WindowsIdentity] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Users_ActiveDirectorySid",
            table: "Users",
            column: "ActiveDirectorySid",
            unique: true,
            filter: "[ActiveDirectorySid] IS NOT NULL");
    }
}
```

## Frontend Integration

### 1. Enhanced Authentication Service

```typescript
// services/auth.ts
export class AuthService {
    private async detectWindowsAuthentication(): Promise<boolean> {
        try {
            const response = await fetch('/api/auth/windows-login', {
                method: 'POST',
                credentials: 'include', // Important for Windows Auth
                headers: {
                    'Content-Type': 'application/json',
                }
            });
            
            if (response.ok) {
                const authData = await response.json();
                this.setAuthToken(authData.token);
                this.setUser(authData.user);
                return true;
            }
        } catch (error) {
            console.log('Windows authentication not available');
        }
        return false;
    }

    async login(email: string, password: string): Promise<boolean> {
        // Try Windows Authentication first
        const windowsAuth = await this.detectWindowsAuthentication();
        if (windowsAuth) {
            return true;
        }

        // Fallback to traditional login
        try {
            const response = await fetch('/api/auth/hybrid-login', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email, password })
            });

            if (response.ok) {
                const authData = await response.json();
                this.setAuthToken(authData.token);
                this.setUser(authData.user);
                return true;
            }
        } catch (error) {
            console.error('Login failed:', error);
        }
        return false;
    }
}
```

## Security Considerations

### 1. Token Security
- JWT tokens generated for Windows authenticated users have the same security as traditional JWT tokens
- Windows identity is embedded in JWT claims for audit purposes
- Tokens include authentication type claim for authorization decisions

### 2. Group Mapping Security
- AD groups are mapped to application roles with configurable rules
- Sensitive operations require specific AD group membership
- Group membership is cached and refreshed periodically

### 3. Fallback Security
- Traditional JWT authentication remains available as fallback
- Windows authentication can be disabled per environment
- Audit logging tracks authentication method used

## Testing Strategy

### 1. Unit Tests
- Test Windows Authentication service with mock AD responses
- Test JWT token generation for Windows users
- Test group mapping logic
- Test fallback authentication scenarios

### 2. Integration Tests
- Test Windows Authentication middleware
- Test hybrid authentication controller
- Test AD synchronization
- Test authentication flow end-to-end

### 3. Manual Testing
- Test with actual Windows domain
- Test with different AD group memberships
- Test fallback scenarios
- Test token refresh and expiration

## Deployment Considerations

### 1. IIS Configuration
```xml
<!-- web.config -->
<system.web>
    <authentication mode="Windows" />
    <authorization>
        <deny users="?" />
    </authorization>
</system.web>
<system.webServer>
    <security>
        <authentication>
            <anonymousAuthentication enabled="false" />
            <windowsAuthentication enabled="true" />
        </authentication>
    </security>
</system.webServer>
```

### 2. Service Account Setup
- Create dedicated service account for AD queries
- Configure minimal required permissions
- Set up secure credential storage
- Configure account password rotation

### 3. Monitoring
- Monitor Windows authentication success/failure rates
- Track AD synchronization performance
- Monitor JWT token generation for Windows users
- Alert on authentication service failures

## Migration Path

### Phase 1: Infrastructure Setup
1. Add Windows Authentication service and interfaces
2. Update User entity with Windows authentication fields
3. Create database migration
4. Configure IIS for Windows Authentication

### Phase 2: Core Implementation
1. Implement Windows Authentication service
2. Create Active Directory integration service
3. Add Windows Authentication middleware
4. Update authentication controller

### Phase 3: Integration
1. Update frontend authentication service
2. Test Windows Authentication flow
3. Test fallback authentication
4. Configure group mapping

### Phase 4: Production Deployment
1. Deploy to staging environment
2. Test with actual AD environment
3. Configure production AD settings
4. Deploy to production with monitoring

## Benefits

1. **Seamless SSO**: Users can access the application without entering credentials
2. **Enhanced Security**: Leverages Windows domain security policies
3. **Centralized Management**: User management through Active Directory
4. **Backward Compatibility**: Existing JWT authentication remains functional
5. **Audit Trail**: Complete tracking of authentication methods used
6. **Flexible Deployment**: Can be enabled/disabled per environment

This implementation provides a robust Windows Authentication integration while maintaining the existing JWT authentication system, offering users the best of both worlds: seamless SSO when available and traditional authentication as a fallback. 