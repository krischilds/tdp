using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Tdp.Api.Services;

public class TokenService
{
    private readonly RsaSecurityKey _key;
    private readonly string _issuer = "http://localhost:5201";
    private readonly string _audience = "tdp-api";

    public TokenService(RsaSecurityKey key)
    {
        _key = key;
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

        var creds = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(15),
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