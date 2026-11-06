using System.Globalization;
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

        var filas = await connection.QueryAsync<ProductoGuardado>("""
            SELECT public_id AS Id, name AS Nombre, description AS Descripcion,
                   COALESCE(price_decimal, CAST(price AS TEXT)) AS Precio, stock, category AS Categoria,
                   created_at AS FechaCreacion
            FROM products
            ORDER BY id DESC
            """);

        List<Product> productos = new List<Product>();
        foreach (ProductoGuardado fila in filas)
        {
            productos.Add(ConvertirProducto(fila));
        }

        return productos;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();

        var fila = await connection.QuerySingleOrDefaultAsync<ProductoGuardado>("""
            SELECT public_id AS Id, name AS Nombre, description AS Descripcion,
                   COALESCE(price_decimal, CAST(price AS TEXT)) AS Precio, stock, category AS Categoria,
                   created_at AS FechaCreacion
            FROM products
            WHERE public_id = @Id
            """, new { Id = id.ToString() });

        if (fila == null)
        {
            return null;
        }

        return ConvertirProducto(fila);
    }

    public async Task<bool> CreateAsync(Product producto)
    {
        using var connection = CreateConnection();

        // La comprobacion y el alta van juntas para evitar duplicados entre solicitudes simultaneas.
        var rows = await connection.ExecuteAsync("""
            INSERT INTO products (public_id, name, description, price, price_decimal, stock, category, created_at)
            SELECT @Id, @Nombre, @Descripcion, @Precio, @PrecioDecimal, @Stock, @Categoria, @FechaCreacion
            WHERE NOT EXISTS (
                SELECT 1 FROM products WHERE name = @Nombre AND category = @Categoria
            );
            """, new
        {
            Id = producto.Id.ToString(),
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            PrecioDecimal = producto.Precio.ToString(CultureInfo.InvariantCulture),
            producto.Stock,
            producto.Categoria,
            FechaCreacion = producto.FechaCreacion.ToString("O", CultureInfo.InvariantCulture)
        });

        return rows > 0;
    }

    public async Task<bool> UpdateAsync(Guid id, ActualizarProductoRequest request)
    {
        using var connection = CreateConnection();

        var rows = await connection.ExecuteAsync("""
            UPDATE products
            SET name = @Nombre,
                description = @Descripcion,
                price = @Precio,
                price_decimal = @PrecioDecimal,
                stock = @Stock,
                category = @Categoria,
                updated_at = datetime('now')
            WHERE public_id = @Id
              AND NOT EXISTS (
                  SELECT 1 FROM products
                  WHERE name = @Nombre AND category = @Categoria AND public_id <> @Id
              )
            """, new
        {
            request.Nombre,
            request.Descripcion,
            request.Precio,
            PrecioDecimal = request.Precio.ToString(CultureInfo.InvariantCulture),
            request.Stock,
            request.Categoria,
            Id = id.ToString()
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();
        var rows = await connection.ExecuteAsync("DELETE FROM products WHERE public_id = @Id",
            new { Id = id.ToString() });

        return rows > 0;
    }

    private static Product ConvertirProducto(ProductoGuardado fila)
    {
        return new Product
        {
            Id = Guid.Parse(fila.Id),
            Nombre = fila.Nombre,
            Descripcion = fila.Descripcion,
            Precio = fila.Precio,
            Stock = fila.Stock,
            Categoria = fila.Categoria ?? string.Empty,
            FechaCreacion = DateTime.Parse(fila.FechaCreacion, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
        };
    }

    // SQLite guarda el Guid y la fecha como texto; el modelo de la API conserva sus tipos.
    private class ProductoGuardado
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
