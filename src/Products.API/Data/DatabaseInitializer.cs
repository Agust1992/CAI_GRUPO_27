using Dapper;
using Microsoft.Data.Sqlite;

namespace Products.API.Data;

public class DatabaseInitializer
{
    private readonly IConfiguration _config;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Initialize()
    {
        var connectionString = _config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                public_id   TEXT,
                name        TEXT    NOT NULL,
                description TEXT,
                price       REAL    NOT NULL DEFAULT 0,
                price_decimal TEXT,
                stock       INTEGER NOT NULL DEFAULT 0,
                category    TEXT,
                created_at  TEXT    NOT NULL DEFAULT (datetime('now')),
                updated_at  TEXT
            );
            """, transaction: transaction);

        // Las bases anteriores se amplian sin borrar tablas ni productos.
        if (connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM pragma_table_info('products') WHERE name = 'public_id'",
            transaction: transaction) == 0)
        {
            connection.Execute("ALTER TABLE products ADD COLUMN public_id TEXT", transaction: transaction);
        }

        if (connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM pragma_table_info('products') WHERE name = 'price_decimal'",
            transaction: transaction) == 0)
        {
            connection.Execute("ALTER TABLE products ADD COLUMN price_decimal TEXT", transaction: transaction);
        }

        var idsAnteriores = connection.Query<long>(
            "SELECT id FROM products WHERE public_id IS NULL", transaction: transaction);
        foreach (long id in idsAnteriores)
        {
            connection.Execute("UPDATE products SET public_id = @PublicId WHERE id = @Id",
                new { PublicId = Guid.NewGuid().ToString(), Id = id }, transaction);
        }

        connection.Execute("CREATE UNIQUE INDEX IF NOT EXISTS ix_products_public_id ON products(public_id)",
            transaction: transaction);
        transaction.Commit();

        _logger.LogInformation("SQLite inicializado correctamente: {Database}", connectionString);
    }
}
