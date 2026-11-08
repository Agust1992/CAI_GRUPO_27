using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks;

// Comprueba que la base de usuarios esté disponible.
public class UsersDatabaseHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public UsersDatabaseHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";

        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy(
                "La base de usuarios está disponible.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "No se pudo conectar con la base de usuarios.",
                exception);
        }
    }
}