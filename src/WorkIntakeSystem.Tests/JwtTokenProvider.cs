using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WorkIntakeSystem.Tests;

public static class JwtTokenProvider
{
    public static string Issuer { get; } = "WorkIntakeSystem-Test";
    public static string Audience { get; } = "WorkIntakeSystem-Test";
    public static SecurityKey SecurityKey { get; }
    public static SigningCredentials SigningCredentials { get; }

    private static readonly JwtSecurityTokenHandler s_tokenHandler = new();
    private static readonly byte[] s_key = new byte[32];

    static JwtTokenProvider()
    {
        // Generate a consistent key for tests
        var keyString = "test-super-secret-jwt-key-with-at-least-32-characters-for-testing";
        s_key = Encoding.ASCII.GetBytes(keyString);
        
        SecurityKey = new SymmetricSecurityKey(s_key) { KeyId = Guid.NewGuid().ToString() };
        SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    }

    public static string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(20),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = SigningCredentials
        };

        var token = s_tokenHandler.CreateToken(tokenDescriptor);
        return s_tokenHandler.WriteToken(token);
    }

    public static string GenerateJwtToken(string userName, string email, string role = "User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("DepartmentId", "1"),
            new("BusinessVerticalId", "1")
        };

        return GenerateJwtToken(claims);
    }
}
