using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Middleware;

public class WindowsAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public WindowsAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if Windows Authentication is enabled and user is authenticated
        if (context.User.Identity?.IsAuthenticated == true && 
            (context.User.Identity.AuthenticationType == "NTLM" || 
             context.User.Identity.AuthenticationType == "Kerberos" ||
             context.User.Identity.AuthenticationType == "Negotiate"))
        {
            var windowsIdentity = context.User.Identity.Name;
            if (!string.IsNullOrEmpty(windowsIdentity))
            {
                try
                {
                    // Resolve the service from the current scope
                    var windowsAuthService = context.RequestServices.GetRequiredService<IWindowsAuthenticationService>();
                    var authResult = await windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);
                    
                    if (authResult.IsAuthenticated && authResult.JwtToken != null)
                    {
                        // Set JWT token in response header for client to capture
                        context.Response.Headers.Add("X-Windows-Auth-Token", authResult.JwtToken);
                        context.Response.Headers.Add("X-Windows-Auth-User", authResult.User?.Name ?? "");
                        context.Response.Headers.Add("X-Windows-Auth-Type", "Windows");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception but don't fail the request
                    Console.WriteLine($"Windows authentication middleware error: {ex.Message}");
                }
            }
        }

        await _next(context);
    }
} 