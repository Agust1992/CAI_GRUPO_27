using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Initialize()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=notifications.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        const string sql = """
            CREATE TABLE IF NOT EXISTS notifications (
                id TEXT NOT NULL PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                tipo TEXT NOT NULL,
                mensaje TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );
            """;

        connection.Execute(sql);
    }
}