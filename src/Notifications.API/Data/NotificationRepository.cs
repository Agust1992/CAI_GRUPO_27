using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data;

// Guarda notificaciones en SQLite.
public class NotificationRepository
{
    private readonly IConfiguration _configuration;

    public NotificationRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=notifications.db";

        return new SqliteConnection(connectionString);
    }

    // Guarda una notificación nueva.
    public async Task CreateAsync(Notification notification)
    {
        using var connection = CreateConnection();

        const string sql = """
            INSERT INTO notifications (
                id, usuario_id, tipo, mensaje, fecha_creacion
            )
            VALUES (
                @Id, @UsuarioId, @Tipo, @Mensaje, @FechaCreacion
            );
            """;

        var datos = new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Tipo,
            notification.Mensaje,
            FechaCreacion = notification.FechaCreacion.ToString(
                "O",
                CultureInfo.InvariantCulture)
        };

        await connection.ExecuteAsync(sql, datos);
    }
}