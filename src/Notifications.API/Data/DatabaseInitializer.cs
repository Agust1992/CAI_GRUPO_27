using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

// Crea la tabla nueva y adapta la anterior sin perder las notificaciones.
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

        var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var columna in connection.Query<ColumnInfo>("PRAGMA table_info(notifications);"))
        {
            columnas.Add(columna.Name);
        }

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

        // Si existe la columna vieja, reconstruye la tabla sin ella.
        if (columnas.Contains("fecha_creacion"))
        {
            using var transaction = connection.BeginTransaction();

            connection.Execute("DROP TABLE IF EXISTS notifications_new;", transaction: transaction);

            connection.Execute("""
                CREATE TABLE notifications_new (
                    id TEXT NOT NULL PRIMARY KEY,
                    usuario_id TEXT NOT NULL,
                    tipo TEXT NOT NULL,
                    mensaje TEXT NOT NULL,
                    estado TEXT NOT NULL,
                    fecha_envio TEXT NOT NULL
                );
                """, transaction: transaction);

            connection.Execute("""
                INSERT INTO notifications_new (
                    id, usuario_id, tipo, mensaje, estado, fecha_envio
                )
                SELECT
                    id,
                    usuario_id,
                    tipo,
                    mensaje,
                    COALESCE(estado, 'Enviada'),
                    COALESCE(fecha_envio, fecha_creacion)
                FROM notifications;
                """, transaction: transaction);

            connection.Execute("DROP TABLE notifications;", transaction: transaction);
            connection.Execute("""
                ALTER TABLE notifications_new RENAME TO notifications;
                """, transaction: transaction);

            transaction.Commit();
        }
    }

    // Representa una columna que SQLite devuelve al consultar la tabla.
    private class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}