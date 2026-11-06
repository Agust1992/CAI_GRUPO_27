using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.DTOs;
using Products.API.Models;

namespace Products.API.Data;

public class ProductRepository
{
    private readonly IConfiguration _config;

    public ProductRepository(IConfiguration config) => _config = config;

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db");

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var connection = CreateConnection();

        return await connection.QueryAsync<Product>("""
            SELECT id, name, description, price, stock, category,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            ORDER BY id DESC
            """);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Product>("""
            SELECT id, name, description, price, stock, category,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products
            WHERE id = @id
            """, new { id });
    }

    public async Task<Product?> CreateAsync(CreateProductRequest request)
    {
        using var connection = CreateConnection();

        // La comprobacion y el alta van juntas para evitar duplicados entre solicitudes simultaneas.
        var id = await connection.ExecuteScalarAsync<int?>("""
            INSERT INTO products (name, description, price, stock, category)
            SELECT @Name, @Description, @Price, @Stock, @Category
            WHERE NOT EXISTS (
                SELECT 1 FROM products WHERE name = @Name AND category = @Category
            )
            RETURNING id;
            """, request);

        if (id == null)
        {
            return null;
        }

        return await GetByIdAsync(id.Value);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request)
    {
        using var connection = CreateConnection();

        var rows = await connection.ExecuteAsync("""
            UPDATE products
            SET name = @Name,
                description = @Description,
                price = @Price,
                stock = @Stock,
                category = @Category,
                updated_at = datetime('now')
            WHERE id = @Id
              AND NOT EXISTS (
                  SELECT 1 FROM products
                  WHERE name = @Name AND category = @Category AND id <> @Id
              )
            """, new
        {
            request.Name,
            request.Description,
            request.Price,
            request.Stock,
            request.Category,
            Id = id
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = CreateConnection();
        var rows = await connection.ExecuteAsync("DELETE FROM products WHERE id = @id", new { id });

        return rows > 0;
    }
}
