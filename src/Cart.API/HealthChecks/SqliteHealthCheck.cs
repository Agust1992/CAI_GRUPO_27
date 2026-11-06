using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    private readonly ILogger<SqliteHealthCheck> _logger;

    public SqliteHealthCheck(IConfiguration config, ILogger<SqliteHealthCheck> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var opciones = new SqliteConnectionStringBuilder(
                _config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db");
            opciones.DefaultTimeout = 3;
            if (opciones.Mode == SqliteOpenMode.ReadOnly)
            {
                return HealthCheckResult.Unhealthy("La base de Carrito no permite escritura.");
            }
            if (opciones.Mode == SqliteOpenMode.ReadWriteCreate)
            {
                opciones.Mode = SqliteOpenMode.ReadWrite;
            }
            using var connection = new SqliteConnection(opciones.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            using var command = connection.CreateCommand();
            command.CommandTimeout = 3;
            command.CommandText = "SELECT user_id, updated_at FROM carts LIMIT 0";
            using (var reader = await command.ExecuteReaderAsync(cancellationToken)) { }
            command.CommandText = "SELECT user_id, product_id, quantity FROM cart_items LIMIT 0";
            using (var reader = await command.ExecuteReaderAsync(cancellationToken)) { }
            return HealthCheckResult.Healthy("La base de Carrito esta disponible.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Fallo el control de salud de la base de Carrito.");
            return HealthCheckResult.Unhealthy("La base de Carrito no esta disponible.");
        }
    }
}
