using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tdp.Api.Infrastructure;
using Tdp.Api.Models;

namespace Tdp.Api.Controllers;

/// <summary>
/// Handles user search and listing operations.
/// </summary>
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly DbConnectionFactory _db;

    /// <summary>
    /// Initializes a new instance of the UsersController.
    /// </summary>
    /// <param name="db">The database connection factory.</param>
    public UsersController(DbConnectionFactory db) { _db = db; }

    /// <summary>
    /// Response model for user list items.
    /// </summary>
    /// <param name="Id">The user ID.</param>
    /// <param name="Email">The user's email address.</param>
    /// <param name="DisplayName">The user's display name.</param>
    public record UserListItem(string Id, string Email, string? DisplayName);

    /// <summary>
    /// Searches for users by email or display name.
    /// </summary>
    /// <param name="q">Optional search query for filtering users.</param>
    /// <returns>A list of users matching the search criteria.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q = null)
    {
        using var conn = _db.CreateConnection();
        if (string.IsNullOrWhiteSpace(q))
        {
            var all = await conn.QueryAsync<UserListItem>(
                "SELECT Id, Email, DisplayName FROM Users ORDER BY Email LIMIT 50");
            return Ok(ResponseDto<IEnumerable<UserListItem>>.Ok(all));
        }
        var term = $"%{q.Trim().ToLowerInvariant()}%";
        var users = await conn.QueryAsync<UserListItem>(@"SELECT Id, Email, DisplayName FROM Users
            WHERE LOWER(Email) LIKE @Term OR LOWER(COALESCE(DisplayName, '')) LIKE @Term
            ORDER BY Email LIMIT 50", new { Term = term });
        return Ok(ResponseDto<IEnumerable<UserListItem>>.Ok(users));
    }
}
