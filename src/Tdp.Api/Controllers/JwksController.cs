using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Tdp.Api.Controllers;

/// <summary>
/// Provides the JSON Web Key Set (JWKS) endpoint for JWT verification.
/// Exposes the public RSA key used to sign JWT tokens.
/// </summary>
[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController : ControllerBase
{
    private readonly RsaSecurityKey _key;

    /// <summary>
    /// Initializes a new instance of the JwksController.
    /// </summary>
    /// <param name="key">The RSA security key used for JWT signing.</param>
    public JwksController(RsaSecurityKey key) { _key = key; }

    /// <summary>
    /// Returns the JSON Web Key Set containing the public key for JWT verification.
    /// </summary>
    /// <returns>The JWKS response with the RSA public key parameters.</returns>
    /// <response code="200">JWKS returned successfully.</response>
    [HttpGet]
    public IActionResult Get()
    {
        var rsa = _key.Rsa;
        var parameters = rsa.ExportParameters(false);
        string Base64Url(byte[] input) => Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var jwk = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = _key.KeyId,
                    alg = "RS256",
                    n = Base64Url(parameters.Modulus!),
                    e = Base64Url(parameters.Exponent!)
                }
            }
        };
        return Ok(jwk);
    }
}