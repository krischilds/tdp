using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tdp.Api.Domain;
using Tdp.Api.Infrastructure;
using Tdp.Api.Models;

namespace Tdp.Api.Controllers;

/// <summary>
/// Handles feature management operations including CRUD operations on features.
/// Requires admin_console feature for create, update, and delete operations.
/// </summary>
[ApiController]
[Route("features")]
public class FeaturesController : ControllerBase
{
    private readonly DbConnectionFactory _db;

    /// <summary>
    /// Initializes a new instance of the FeaturesController.
    /// </summary>
    /// <param name="db">The database connection factory.</param>
    public FeaturesController(DbConnectionFactory db) { _db = db; }

    /// <summary>
    /// Checks if a user has the admin_console feature.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user has admin privileges; otherwise, false.</returns>
    private async Task<bool> IsAdminAsync(string userId)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM UserFeatures uf
            INNER JOIN Features f ON f.Id=uf.FeatureId
            WHERE uf.UserId=@UserId AND f.Name='admin_console'",
            new { UserId = userId });
        return count > 0;
    }

    /// <summary>
    /// Retrieves all features.
    /// </summary>
    /// <returns>A list of all features ordered by name.</returns>
    /// <response code="200">Features retrieved successfully.</response>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        using var conn = _db.CreateConnection();
        var features = await conn.QueryAsync<Feature>("SELECT * FROM Features ORDER BY Name");
        return Ok(ResponseDto<IEnumerable<Feature>>.Ok(features));
    }

    /// <summary>
    /// Retrieves a specific feature by ID.
    /// </summary>
    /// <param name="id">The feature ID.</param>
    /// <returns>The requested feature.</returns>
    /// <response code="200">Feature retrieved successfully.</response>
    /// <response code="404">Feature not found.</response>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    /// <summary>
    /// Request model for creating or updating a feature.
    /// </summary>
    /// <param name="Name">The feature name (must be unique).</param>
    /// <param name="Description">Optional description of the feature.</param>
    public record UpsertFeatureRequest(string Name, string? Description);

    /// <summary>
    /// Creates a new feature. Requires admin_console feature.
    /// </summary>
    /// <param name="req">The feature creation request.</param>
    /// <returns>The created feature.</returns>
    /// <response code="200">Feature created successfully.</response>
    /// <response code="403">Forbidden - requires admin privileges.</response>
    /// <response code="409">Feature name already exists.</response>        using var conn = _db.CreateConnection();
        var feature = await conn.QuerySingleOrDefaultAsync<Feature>("SELECT * FROM Features WHERE Id=@Id", new { Id = id });
        if (feature == null) return Problem(title: "Feature not found", statusCode: 404);
        return Ok(ResponseDto<Feature>.Ok(feature));
    }

    public record UpsertFeatureRequest(string Name, string? Description);

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertFeatureRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Problem(title: "Unauthorized", statusCode: 401);
        if (!await IsAdminAsync(userId)) return Problem(title: "Forbidden", statusCode: 403);

        using var conn = _db.CreateConnection();
        var id = Guid.NewGuid().ToString();
        var createdAt = DateTime.UtcNow.ToString("O");
        try
    /// <summary>
    /// Updates an existing feature. Requires admin_console feature.
    /// </summary>
    /// <param name="id">The feature ID to update.</param>
    /// <param name="req">The feature update request.</param>
    /// <returns>The updated feature.</returns>
    /// <response code="200">Feature updated successfully.</response>
    /// <response code="403">Forbidden - requires admin privileges.</response>
    /// <response code="404">Feature not found.</response>
        {
            await conn.ExecuteAsync(@"INSERT INTO Features (Id, Name, Description, CreatedAt)
                VALUES (@Id, @Name, @Description, @CreatedAt);",
                new { Id = id, req.Name, req.Description, CreatedAt = createdAt });
        }
        catch (Exception)
        {
            return Problem(title: "Feature name must be unique", statusCode: 409);
        }
        var feature = await conn.QuerySingleAsync<Feature>("SELECT * FROM Features WHERE Id=@Id", new { Id = id });
        return Ok(ResponseDto<Feature>.Created(feature));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpsertFeatureRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Problem(title: "Unauthorized", statusCode: 401);
        if (!await IsAdminAsync(userId)) return Problem(title: "Forbidden", statusCode: 403);
/// <summary>
    /// Deletes a feature. Requires admin_console feature.
    /// </summary>
    /// <param name="id">The feature ID to delete.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">Feature deleted successfully.</response>
    /// <response code="403">Forbidden - requires admin privileges.</response>
    /// <response code="404">Feature not found.</response>
    
        using var conn = _db.CreateConnection();
        var updated = await conn.ExecuteAsync("UPDATE Features SET Name=@Name, Description=@Description WHERE Id=@Id",
            new { Id = id, req.Name, req.Description });
        if (updated == 0) return Problem(title: "Feature not found", statusCode: 404);
        var feature = await conn.QuerySingleAsync<Feature>("SELECT * FROM Features WHERE Id=@Id", new { Id = id });
        return Ok(ResponseDto<Feature>.Ok(feature));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Problem(title: "Unauthorized", statusCode: 401);
        if (!await IsAdminAsync(userId)) return Problem(title: "Forbidden", statusCode: 403);

        using var conn = _db.CreateConnection();
        var deleted = await conn.ExecuteAsync("DELETE FROM Features WHERE Id=@Id", new { Id = id });
        if (deleted == 0) return Problem(title: "Feature not found", statusCode: 404);
        return Ok(ResponseDto<object>.Ok(null, message: "Deleted"));
    }
}