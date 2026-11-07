using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.API.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    private readonly ILogger<SqliteHealthCheck> _logger;

    public SqliteHealthCheck(IConfiguration config, ILogger<SqliteHealthCheck> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            var opciones = new SqliteConnectionStringBuilder(connectionString);
            opciones.DefaultTimeout = 3;
            if (opciones.Mode == SqliteOpenMode.ReadOnly)
            {
                return HealthCheckResult.Unhealthy("La base de Productos no permite escritura.");
            }
            if (opciones.Mode == SqliteOpenMode.ReadWriteCreate)
            {
                // Un control de salud no debe crear una base faltante y declararla sana.
                opciones.Mode = SqliteOpenMode.ReadWrite;
            }

            using var connection = new SqliteConnection(opciones.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT id, public_id, name, description, price, price_decimal,
                       stock, category, created_at, updated_at
                FROM products
                LIMIT 0
                """;
            command.CommandTimeout = 3;
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return HealthCheckResult.Healthy("La base de Productos esta disponible.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Fallo el control de disponibilidad de la base de Productos.");
            return HealthCheckResult.Unhealthy("La base de Productos no esta disponible.");
        }
    }
}
