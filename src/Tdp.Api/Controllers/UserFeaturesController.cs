using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tdp.Api.Infrastructure;
using Tdp.Api.Models;
using Tdp.Api.Domain;

namespace Tdp.Api.Controllers;

/// <summary>
/// Handles user-feature assignment operations including viewing and managing feature assignments.
/// Admin operations require the admin_console feature.
/// </summary>
[ApiController]
[Route("users")]
public class UserFeaturesController : ControllerBase
{
    private readonly DbConnectionFactory _db;

    /// <summary>
    /// Initializes a new instance of the UserFeaturesController.
    /// </summary>
    /// <param name="db">The database connection factory.</param>
    public UserFeaturesController(DbConnectionFactory db) { _db = db; }

    /// <summary>
    /// Checks if a user has the admin_console feature.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="db">The database connection factory.</param>
    /// <returns>True if the user has admin privileges; otherwise, false.</returns>
    private async Task<bool> IsAdminAsync(string userId, DbConnectionFactory db)
    {
        using var conn = db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM UserFeatures uf
            INNER JOIN Features f ON f.Id=uf.FeatureId
            WHERE uf.UserId=@UserId AND f.Name='admin_console'",
            new { UserId = userId });
        return count > 0;
    }

    /// <summary>
    /// Retrieves the current user's assigned features.
    /// </summary>
    /// <returns>A list of features assigned to the current user.</returns>
    /// <response code="200">Features retrieved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    [Authorize]
    [HttpGet("me/features")]
    public async Task<IActionResult> GetMyFeatures()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Problem(title: "Unauthorized", statusCode: 401);
        using var conn = _db.CreateConnection();
        var features = await conn.QueryAsync<Feature>(@"SELECT f.* FROM Features f
            INNER JOIN UserFeatures uf ON uf.FeatureId=f.Id
            WHERE uf.UserId=@UserId ORDER BY f.Name", new { UserId = userId });
        return Ok(ResponseDto<IEnumerable<Feature>>.Ok(features));
    }

    /// <summary>
    /// Retrieves features assigned to a specific user.
    /// </summary>
    /// <param name="userId">The user ID to get features for.</param>
    /// <returns>A list of features assigned to the specified user.</returns>
    /// <response code="200">Features retrieved successfully.</response>
    [Authorize]
    [HttpGet("{userId}/features")]
    public async Task<IActionResult> GetUserFeatures(string userId)
    {
        using var conn = _db.CreateConnection();
        var features = await conn.QueryAsync<Feature>(@"SELECT f.* FROM Features f
            INNER JOIN UserFeatures uf ON uf.FeatureId=f.Id WHERE uf.UserId=@UserId ORDER BY f.Name",
            new { UserId = userId });
        return Ok(ResponseDto<IEnumerable<Feature>>.Ok(features));
    }

    /// <summary>
    /// Assigns a feature to another user. Requires admin_console feature.
    /// </summary>
    /// <param name="userId">The user ID to assign the feature to.</param>
    /// <param name="featureId">The feature ID to assign.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Feature assigned successfully.</response>
    /// <response code="403">Forbidden - requires admin privileges.</response>
    /// <response code="404">User or feature not found.</response>
    [Authorize]
    [HttpPost("{userId}/features/{featureId}")]
    public async Task<IActionResult> AddFeature(string userId, string featureId)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(adminId)) return Problem(title: "Unauthorized", statusCode: 401);
        if (!await IsAdminAsync(adminId, _db)) return Problem(title: "Forbidden", statusCode: 403);

        using var conn = _db.CreateConnection();
    /// <summary>
    /// Assigns a feature to the current user.
    /// </summary>
    /// <param name="featureId">The feature ID to assign.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Feature assigned successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Feature not found.</response>
        var countUser = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE Id=@Id", new { Id = userId });
        var countFeature = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Features WHERE Id=@Id", new { Id = featureId });
        if (countUser == 0 || countFeature == 0) return Problem(title: "Not found", statusCode: 404);
        await conn.ExecuteAsync(@"INSERT OR IGNORE INTO UserFeatures (UserId, FeatureId, AssignedAt)
            VALUES (@UserId, @FeatureId, @AssignedAt);",
            new { UserId = userId, FeatureId = featureId, AssignedAt = DateTime.UtcNow.ToString("O") });
        return Ok(ResponseDto<object>.Ok(null, message: "Assigned"));
    }

    [Authorize]
    [HttpPost("me/features/{featureId}")]
    public async Task<IActionResult> AddMyFeature(string featureId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Problem(title: "Unauthorized", statusCode: 401);
        using var conn = _db.CreateConnection();
    /// <summary>
    /// Removes a feature from another user. Requires admin_console feature.
    /// </summary>
    /// <param name="userId">The user ID to remove the feature from.</param>
    /// <param name="featureId">The feature ID to remove.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Feature removed successfully.</response>
    /// <response code="403">Forbidden - requires admin privileges.</response>
    /// <response code="404">Relation not found.</response>
        var countFeature = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Features WHERE Id=@Id", new { Id = featureId });
        if (countFeature == 0) return Problem(title: "Feature not found", statusCode: 404);
        await conn.ExecuteAsync(@"INSERT OR IGNORE INTO UserFeatures (UserId, FeatureId, AssignedAt)
            VALUES (@UserId, @FeatureId, @AssignedAt);",
            new { UserId = userId, FeatureId = featureId, AssignedAt = DateTime.UtcNow.ToString("O") });
        return Ok(ResponseDto<object>.Ok(null, message: "Assigned"));
    }

    [Authorize]
    [HttpDelete("{userId}/features/{featureId}")]
    public async Task<IActionResult> RemoveFeature(string userId, string featureId)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(adminId)) return Problem(title: "Unauthorized", statusCode: 401);
        if (!await IsAdminAsync(adminId, _db)) return Problem(title: "Forbidden", statusCode: 403);

        using var conn = _db.CreateConnection();
        var deleted = await conn.ExecuteAsync("DELETE FROM UserFeatures WHERE UserId=@UserId AND FeatureId=@FeatureId",
            new { UserId = userId, FeatureId = featureId });
        if (deleted == 0) return Problem(title: "Relation not found", statusCode: 404);
        return Ok(ResponseDto<object>.Ok(null, message: "Removed"));
    }

    [Authorize]
    [HttpDelete("me/features/{featureId}")]
    public async Task<IActionResult> RemoveMyFeature(string featureId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Problem(title: "Unauthorized", statusCode: 401);
        using var conn = _db.CreateConnection();
        var deleted = await conn.ExecuteAsync("DELETE FROM UserFeatures WHERE UserId=@UserId AND FeatureId=@FeatureId",
            new { UserId = userId, FeatureId = featureId });
        if (deleted == 0) return Problem(title: "Relation not found", statusCode: 404);
        return Ok(ResponseDto<object>.Ok(null, message: "Removed"));
    }
}