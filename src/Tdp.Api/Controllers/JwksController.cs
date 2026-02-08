using Microsoft.AspNetCore.Mvc;

namespace Tdp.Api.Controllers;

/// <summary>
/// Provides information about JWT signing configuration.
/// Note: With symmetric key signing (HMAC), the key is not publicly exposed.
/// </summary>
[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController : ControllerBase
{
    /// <summary>
    /// Returns information about the JWT signing configuration.
    /// </summary>
    /// <returns>A message indicating symmetric signing is used.</returns>
    /// <response code="200">Configuration info returned.</response>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "This API uses symmetric key signing (HS256). The signing key is not publicly exposed.",
            algorithm = "HS256"
        });
    }
}