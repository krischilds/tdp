using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Tdp.Api.Services;

public class TokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    public int RefreshTokenExpirationDays { get; }

    public TokenService(IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        _issuer = configuration["Jwt:Issuer"] ?? "http://localhost:5201";
        _audience = configuration["Jwt:Audience"] ?? "tdp-api";
        _accessTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15);
        RefreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 14);
    }

    public (string accessToken, DateTime expiresAt) CreateAccessToken(string userId, string email, string? name, IEnumerable<string>? permissions = null)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, name ?? email)
        };
        if (permissions != null)
        {
            foreach (var p in permissions)
                claims.Add(new Claim("permissions", p));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: creds
        );
        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(token), token.ValidTo);
    }

    public string CreateRefreshToken()
    {
        var bytes = new byte[64];
        Random.Shared.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}