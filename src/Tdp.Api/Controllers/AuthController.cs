using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tdp.Api.Infrastructure;
using Tdp.Api.Models;
using Tdp.Api.Domain;
using Tdp.Api.Services;

namespace Tdp.Api.Controllers;

/// <summary>
/// Handles authentication-related operations including user registration, login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly DbConnectionFactory _db;
    private readonly TokenService _tokens;

    /// <summary>
    /// Initializes a new instance of the AuthController.
    /// </summary>
    /// <param name="db">The database connection factory.</param>
    /// <param name="tokens">The token service for JWT operations.</param>
    public AuthController(DbConnectionFactory db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    /// <summary>
    /// Request model for user registration.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    /// <param name="DisplayName">Optional display name for the user.</param>
    public record RegisterRequest(string Email, string Password, string? DisplayName);

    /// <summary>
    /// Request model for user login.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    /// <param name="DeviceInfo">Optional device information for tracking.</param>
    public record LoginRequest(string Email, string Password, string? DeviceInfo);

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="req">The registration request containing email, password, and optional display name.</param>
    /// <returns>A response indicating success or failure of registration.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        using var conn = _db.CreateConnection();
        var exists = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE Email=@Email", new { req.Email });
        if (exists > 0)
        {
            return Problem(title: "Email already registered", statusCode: 409);
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = req.Email.Trim().ToLowerInvariant(),
            PasswordHash = AuthService.HashPassword(req.Password),
            DisplayName = req.DisplayName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await conn.ExecuteAsync(@"INSERT INTO Users (Id, Email, PasswordHash, DisplayName, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @Email, @PasswordHash, @DisplayName, 1, @CreatedAt, @UpdatedAt);", new
        {
            user.Id,
            user.Email,
            user.PasswordHash,
            user.DisplayName,
            CreatedAt = user.CreatedAt.ToString("O"),
            UpdatedAt = user.UpdatedAt.ToString("O")
        });

        return Ok(ResponseDto<object>.Created(new { userId = user.Id }, HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="req">The login request containing email, password, and optional device info.</param>
    /// <returns>A response containing access token, refresh token, and expiration info.</returns>
    /// <response code="200">Login successful with tokens.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        using var conn = _db.CreateConnection();
        var user = await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Email=@Email", new { Email = req.Email.Trim().ToLowerInvariant() });
        if (user == null || !AuthService.VerifyPassword(req.Password, user.PasswordHash))
        {
            return Problem(title: "Invalid credentials", statusCode: 401);
        }

        var (accessToken, expires) = _tokens.CreateAccessToken(user.Id, user.Email, user.DisplayName);
        var refresh = _tokens.CreateRefreshToken();
        var refreshHash = AuthService.HashRefreshToken(refresh);
        var rt = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            TokenHash = refreshHash,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            DeviceInfo = req.DeviceInfo
        };
        await conn.ExecuteAsync(@"INSERT INTO RefreshTokens (Id, UserId, TokenHash, IssuedAt, ExpiresAt, DeviceInfo)
            VALUES (@Id, @UserId, @TokenHash, @IssuedAt, @ExpiresAt, @DeviceInfo);", new
        {
            rt.Id,
            rt.UserId,
            TokenHash = rt.TokenHash,
            IssuedAt = rt.IssuedAt.ToString("O"),
            ExpiresAt = rt.ExpiresAt.ToString("O"),
            rt.DeviceInfo
        });

        return Ok(ResponseDto<object>.Ok(new
        {
            accessToken,
            refreshToken = refresh,
            expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds
        }, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Request model for token refresh.
    /// </summary>
    /// <param name="RefreshToken">The refresh token to exchange for new tokens.</param>
    /// <param name="DeviceInfo">Optional device information for tracking.</param>
    public record RefreshRequest(string RefreshToken, string? DeviceInfo);

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// </summary>
    /// <param name="req">The refresh request containing the refresh token and device info.</param>
    /// <returns>A response with new access and refresh tokens.</returns>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        using var conn = _db.CreateConnection();
        var hash = AuthService.HashRefreshToken(req.RefreshToken);
        var token = await conn.QuerySingleOrDefaultAsync<RefreshToken>("SELECT * FROM RefreshTokens WHERE TokenHash=@TokenHash", new { TokenHash = hash });
        if (token == null || token.RevokedAt != null || token.ExpiresAt <= DateTime.UtcNow)
        {
            return Problem(title: "Invalid refresh token", statusCode: 401);
        }
        var user = await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id=@Id", new { Id = token.UserId });
        if (user == null)
        {
            return Problem(title: "User not found", statusCode: 404);
        }

        // Rotate refresh token
        var newRefresh = _tokens.CreateRefreshToken();
        var newHash = AuthService.HashRefreshToken(newRefresh);
        var newRtId = Guid.NewGuid().ToString();

        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync("UPDATE RefreshTokens SET RevokedAt=@RevokedAt, ReplacedByTokenId=@Replaced WHERE Id=@Id",
            new { RevokedAt = DateTime.UtcNow.ToString("O"), Replaced = newRtId, Id = token.Id }, tx);
        await conn.ExecuteAsync(@"INSERT INTO RefreshTokens (Id, UserId, TokenHash, IssuedAt, ExpiresAt, DeviceInfo)
            VALUES (@Id, @UserId, @TokenHash, @IssuedAt, @ExpiresAt, @DeviceInfo);",
            new
            {
                Id = newRtId,
                UserId = user.Id,
                TokenHash = newHash,
                IssuedAt = DateTime.UtcNow.ToString("O"),
                ExpiresAt = DateTime.UtcNow.AddDays(14).ToString("O"),
                DeviceInfo = req.DeviceInfo
            }, tx);
        tx.Commit();

        var (accessToken, expires) = _tokens.CreateAccessToken(user.Id, user.Email, user.DisplayName);
        return Ok(ResponseDto<object>.Ok(new
        {
            accessToken,
            refreshToken = newRefresh,
            expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds
        }, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Request model for user logout.
    /// </summary>
    /// <param name="RefreshToken">The refresh token to invalidate.</param>
    public record LogoutRequest(string RefreshToken);

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    /// <param name="req">The logout request containing the refresh token.</param>
    /// <returns>A response indicating successful logout.</returns>
    /// <response code="200">Logout successful.</response>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
    {
        using var conn = _db.CreateConnection();
        var hash = AuthService.HashRefreshToken(req.RefreshToken);
        await conn.ExecuteAsync("UPDATE RefreshTokens SET RevokedAt=@RevokedAt WHERE TokenHash=@TokenHash",
            new { RevokedAt = DateTime.UtcNow.ToString("O"), TokenHash = hash });
        return Ok(ResponseDto<object>.Ok(null, status: 200, message: "Logged out", traceId: HttpContext.TraceIdentifier));
    }
}