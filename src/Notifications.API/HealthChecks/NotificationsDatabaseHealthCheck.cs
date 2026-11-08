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
            command.CommandText = """
                SELECT name
                FROM sqlite_master
                WHERE type = 'table' AND name = 'notifications';
                """;

            var table = await command.ExecuteScalarAsync(cancellationToken);

            if (table is null)
            {
                return HealthCheckResult.Unhealthy(
                    "No existe la tabla de notificaciones.");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "No se pudo acceder a la base de notificaciones.",
                exception);
        }
    }
}