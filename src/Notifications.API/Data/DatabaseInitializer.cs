using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

// Crea la tabla y agrega los campos nuevos si la base ya existía.
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

        const string createTableSql = """
            CREATE TABLE IF NOT EXISTS notifications (
                id TEXT NOT NULL PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                tipo TEXT NOT NULL,
                mensaje TEXT NOT NULL,
                estado TEXT NOT NULL,
                fecha_envio TEXT NOT NULL
            );
            """;

        connection.Execute(createTableSql);

        var columnas = connection
            .Query<ColumnInfo>("PRAGMA table_info(notifications);")
            .Select(columna => columna.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!columnas.Contains("estado"))
        {
            connection.Execute("""
                ALTER TABLE notifications
                ADD COLUMN estado TEXT NOT NULL DEFAULT 'Enviada';
                """);
        }

        if (!columnas.Contains("fecha_envio"))
        {
            connection.Execute("""
                ALTER TABLE notifications
                ADD COLUMN fecha_envio TEXT;
                """);

            if (columnas.Contains("fecha_creacion"))
            {
                connection.Execute("""
                    UPDATE notifications
                    SET fecha_envio = fecha_creacion
                    WHERE fecha_envio IS NULL;
                    """);
            }
        }
    }

    // Representa una columna devuelta por SQLite.
    private class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}