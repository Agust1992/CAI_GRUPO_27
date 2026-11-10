using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data;

// Guarda y busca notificaciones en SQLite.
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
                id, usuario_id, tipo, mensaje, estado, fecha_envio
            )
            VALUES (
                @Id, @UsuarioId, @Tipo, @Mensaje, @Estado, @FechaEnvio
            );
            """;

        var datos = new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Tipo,
            notification.Mensaje,
            notification.Estado,
            FechaEnvio = notification.FechaEnvio.ToString(
                "O",
                CultureInfo.InvariantCulture)
        };

        await connection.ExecuteAsync(sql, datos);
    }

    // Busca las notificaciones de un usuario.
    public async Task<List<Notification>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        using var connection = CreateConnection();

        const string sql = """
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                tipo AS Tipo,
                mensaje AS Mensaje,
                estado AS Estado,
                fecha_envio AS FechaEnvio
            FROM notifications
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_envio DESC;
            """;

        var filas = await connection.QueryAsync<NotificationData>(
            sql,
            new { UsuarioId = usuarioId.ToString() });

        var notificaciones = new List<Notification>();
        foreach (var fila in filas)
        {
            notificaciones.Add(new Notification
            {
                Id = Guid.Parse(fila.Id),
                UsuarioId = Guid.Parse(fila.UsuarioId),
                Tipo = fila.Tipo,
                Mensaje = fila.Mensaje,
                Estado = fila.Estado,
                FechaEnvio = DateTime.Parse(
                    fila.FechaEnvio,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind)
            });
        }

        return notificaciones;
    }

    // Recibe los datos de SQLite antes de convertirlos al modelo.
    private class NotificationData
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string FechaEnvio { get; set; } = string.Empty;
    }
}