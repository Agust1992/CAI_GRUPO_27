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
                fecha_creacion AS FechaCreacion
            FROM notifications
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_creacion DESC;
            """;

        var filas = await connection.QueryAsync<NotificationData>(
            sql,
            new { UsuarioId = usuarioId.ToString() });

        return filas.Select(fila => new Notification
        {
            Id = Guid.Parse(fila.Id),
            UsuarioId = Guid.Parse(fila.UsuarioId),
            Tipo = fila.Tipo,
            Mensaje = fila.Mensaje,
            FechaCreacion = DateTime.Parse(
                fila.FechaCreacion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind)
        }).ToList();
    }

    // Recibe los datos de SQLite antes de convertirlos al modelo.
    private class NotificationData
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
    }
}