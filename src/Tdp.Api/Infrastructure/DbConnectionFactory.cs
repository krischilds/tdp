using System.Data;
using System.Data.SQLite;

namespace Tdp.Api.Infrastructure;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "tdp.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath};Version=3;Pooling=True;Max Pool Size=100";
    }

    public IDbConnection CreateConnection()
    {
        var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;";
        pragma.ExecuteNonQuery();
        return conn;
    }
}