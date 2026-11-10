using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks;

// Comprueba la conexión y la estructura de la base de usuarios.
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

            var columnas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(users);";

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                columnas.Add(reader.GetString(1));
            }

            var columnasEsperadas = new[]
            {
                "id",
                "nombre",
                "apellido",
                "email",
                "password_hash",
                "fecha_registro",
                "activo",
                "intentos_fallidos"
            };

            var faltantes = new List<string>();
            foreach (var columna in columnasEsperadas)
            {
                if (!columnas.Contains(columna))
                {
                    faltantes.Add(columna);
                }
            }

            if (faltantes.Count > 0)
            {
                return HealthCheckResult.Unhealthy(
                    $"La tabla users no existe o le faltan columnas: {string.Join(", ", faltantes)}.");
            }

            return HealthCheckResult.Healthy(
                "La base de usuarios y la tabla users están disponibles.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "No se pudo comprobar la base de usuarios.",
                exception);
        }
    }
}