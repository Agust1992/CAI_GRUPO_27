using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data;

public class DatabaseInitializer
{
    private readonly IConfiguration _config;

    public DatabaseInitializer(IConfiguration config)
    {
        _config = config;
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(
            _config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db;Foreign Keys=True");
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS carts (
                user_id TEXT PRIMARY KEY,
                updated_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS cart_items (
                user_id TEXT NOT NULL,
                product_id TEXT NOT NULL,
                quantity INTEGER NOT NULL CHECK (quantity > 0),
                PRIMARY KEY (user_id, product_id),
                FOREIGN KEY (user_id) REFERENCES carts(user_id) ON DELETE CASCADE
            );
            """);
    }
}
