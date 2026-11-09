using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

// Comprueba que la base y la tabla de notificaciones estén disponibles.
public class NotificationsDatabaseHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public NotificationsDatabaseHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(notifications);";

            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    columnas.Add(reader.GetString(1));
                }
            }

            var columnasEsperadas = new[]
            {
                "id", "usuario_id", "tipo", "mensaje", "estado", "fecha_envio"
            };
            var columnasFaltantes = columnasEsperadas
                .Where(columna => !columnas.Contains(columna))
                .ToArray();

            if (columnasFaltantes.Length > 0)
            {
                return HealthCheckResult.Unhealthy(
                    columnas.Count == 0
                        ? "No existe la tabla de notificaciones."
                        : $"A la tabla de notificaciones le faltan columnas: {string.Join(", ", columnasFaltantes)}.");
            }

            return HealthCheckResult.Healthy(
                "La base y la tabla de notificaciones están disponibles.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "No se pudo acceder a la base de notificaciones.",
                exception);
        }
    }
}