using Dapper;

namespace Tdp.Api.Infrastructure;

public class DbInitializer
{
    private readonly DbConnectionFactory _factory;

    public DbInitializer(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        using var conn = _factory.CreateConnection();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Users (
            Id TEXT PRIMARY KEY,
            Email TEXT NOT NULL UNIQUE,
            PasswordHash TEXT NOT NULL,
            DisplayName TEXT,
            IsActive INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );", transaction: tx);

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Features (
            Id TEXT PRIMARY KEY,
            Name TEXT NOT NULL UNIQUE,
            Description TEXT,
            CreatedAt TEXT NOT NULL
        );", transaction: tx);

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS UserFeatures (
            UserId TEXT NOT NULL,
            FeatureId TEXT NOT NULL,
            AssignedAt TEXT NOT NULL,
            PRIMARY KEY (UserId, FeatureId),
            FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
            FOREIGN KEY(FeatureId) REFERENCES Features(Id) ON DELETE CASCADE
        );", transaction: tx);

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS RefreshTokens (
            Id TEXT PRIMARY KEY,
            UserId TEXT NOT NULL,
            TokenHash TEXT NOT NULL,
            IssuedAt TEXT NOT NULL,
            ExpiresAt TEXT NOT NULL,
            RevokedAt TEXT,
            ReplacedByTokenId TEXT,
            DeviceInfo TEXT,
            FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
        );", transaction: tx);

        // Seed features
        var features = new[]
        {
            ("beta_access", "Access to beta features"),
            ("dark_mode", "Enable dark theme"),
            ("analytics", "View analytics"),
            ("pro_reports", "Generate pro reports"),
            ("admin_console", "Admin tools")
        };

        foreach (var (name, desc) in features)
        {
            await conn.ExecuteAsync(@"INSERT OR IGNORE INTO Features (Id, Name, Description, CreatedAt)
                VALUES (@Id, @Name, @Description, @CreatedAt);",
                new { Id = Guid.NewGuid().ToString(), Name = name, Description = desc, CreatedAt = DateTime.UtcNow.ToString("O") }, tx);
        }

        // Seed admin user
        var adminEmail = "admin@tdp.local";
        var userExists = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE Email=@Email", new { Email = adminEmail }, tx);
        if (userExists == 0)
        {
            // Use a temporary known password for admin; should be changed in production.
            var passwordHash = Tdp.Api.Services.AuthService.HashPassword("Admin123!");
            var userId = Guid.NewGuid().ToString();
            await conn.ExecuteAsync(@"INSERT INTO Users (Id, Email, PasswordHash, DisplayName, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Id, @Email, @PasswordHash, @DisplayName, 1, @CreatedAt, @UpdatedAt);",
                new
                {
                    Id = userId,
                    Email = adminEmail,
                    PasswordHash = passwordHash,
                    DisplayName = "Administrator",
                    CreatedAt = DateTime.UtcNow.ToString("O"),
                    UpdatedAt = DateTime.UtcNow.ToString("O")
                }, tx);

            // Grant admin_console
            var featureId = await conn.QuerySingleOrDefaultAsync<string>("SELECT Id FROM Features WHERE Name=@Name", new { Name = "admin_console" }, tx);
            if (!string.IsNullOrEmpty(featureId))
            {
                await conn.ExecuteAsync(@"INSERT OR IGNORE INTO UserFeatures (UserId, FeatureId, AssignedAt)
                    VALUES (@UserId, @FeatureId, @AssignedAt);",
                    new { UserId = userId, FeatureId = featureId, AssignedAt = DateTime.UtcNow.ToString("O") }, tx);
            }
        }

        tx.Commit();
    }
}