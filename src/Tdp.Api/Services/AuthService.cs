using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;

namespace Tdp.Api.Services;

/// <summary>
/// Provides authentication-related utility methods for password hashing, verification, and token hashing.
/// </summary>
public static class AuthService
{
    /// <summary>
    /// Hashes a password using Argon2id with a random salt.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A string containing the hashed password in the format: argon2id${salt}${hash}.</returns>
    public static string HashPassword(string password)
    {
        // Simple Argon2id hashing
        var salt = RandomNumberGenerator.GetBytes(16);
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 2,
            MemorySize = 1024 * 64, // 64MB
            Iterations = 4
        };
        var hash = argon2.GetBytes(32);
        return $"argon2id${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a plain-text password against a stored hashed password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="stored">The stored hashed password string.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('$');
        if (parts.Length != 3 || parts[0] != "argon2id") return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 2,
            MemorySize = 1024 * 64,
            Iterations = 4
        };
        var hash = argon2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(hash, expected);
    }

    /// <summary>
    /// Hashes a refresh token using SHA256 for secure storage.
    /// </summary>
    /// <param name="token">The refresh token to hash.</param>
    /// <returns>The Base64-encoded SHA256 hash of the token.</returns>
    public static string HashRefreshToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}