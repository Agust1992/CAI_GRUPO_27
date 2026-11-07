using System.Globalization;
using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using Carrito = Cart.API.Models.Cart;

namespace Cart.API.Data;

public class CarritoRepository
{
    private readonly IConfiguration _config;

    public CarritoRepository(IConfiguration config)
    {
        _config = config;
    }

    private SqliteConnection CreateConnection()
    {
        var opciones = new SqliteConnectionStringBuilder(
            _config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db");
        opciones.ForeignKeys = true;
        return new SqliteConnection(opciones.ConnectionString);
    }

    public async Task<Carrito?> ObtenerAsync(Guid usuarioId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction(deferred: true);
        var carrito = await LeerAsync(connection, transaction, usuarioId);
        transaction.Commit();
        return carrito;
    }

    public async Task<Carrito?> AgregarAsync(Guid usuarioId, Guid productoId, int cantidad, int stock)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        // La lectura y la suma quedan juntas para no perder cambios simultaneos.
        using var transaction = connection.BeginTransaction();
        var carrito = await LeerAsync(connection, transaction, usuarioId);
        int cantidadActual = 0;
        if (carrito != null)
        {
            foreach (CartItem item in carrito.Items)
            {
                if (item.ProductoId == productoId)
                {
                    cantidadActual = item.Cantidad;
                    break;
                }
            }
        }

        if (cantidad > stock - cantidadActual)
        {
            return null;
        }

        if (carrito == null)
        {
            await connection.ExecuteAsync("INSERT INTO carts (user_id, updated_at) VALUES (@UsuarioId, @Fecha)",
                new { UsuarioId = usuarioId.ToString(), Fecha = FechaActual() }, transaction);
        }

        if (cantidadActual == 0)
        {
            await connection.ExecuteAsync("""
                INSERT INTO cart_items (user_id, product_id, quantity)
                VALUES (@UsuarioId, @ProductoId, @Cantidad)
                """, new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString(), Cantidad = cantidad }, transaction);
        }
        else
        {
            await connection.ExecuteAsync("""
                UPDATE cart_items SET quantity = @Cantidad
                WHERE user_id = @UsuarioId AND product_id = @ProductoId
                """, new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString(), Cantidad = cantidadActual + cantidad }, transaction);
        }

        await ActualizarFechaAsync(connection, transaction, usuarioId);
        var resultado = await LeerAsync(connection, transaction, usuarioId);
        transaction.Commit();
        return resultado;
    }

    public async Task<Carrito?> ActualizarCantidadAsync(Guid usuarioId, Guid productoId, int cantidad)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        int filas = await connection.ExecuteAsync("""
            UPDATE cart_items SET quantity = @Cantidad
            WHERE user_id = @UsuarioId AND product_id = @ProductoId
            """, new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString(), Cantidad = cantidad }, transaction);
        if (filas == 0)
        {
            return null;
        }

        await ActualizarFechaAsync(connection, transaction, usuarioId);
        var carrito = await LeerAsync(connection, transaction, usuarioId);
        transaction.Commit();
        return carrito;
    }

    public async Task<bool> EliminarItemAsync(Guid usuarioId, Guid productoId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        var carrito = await LeerAsync(connection, transaction, usuarioId);
        if (carrito == null)
        {
            return false;
        }

        int filas = await connection.ExecuteAsync("""
            DELETE FROM cart_items WHERE user_id = @UsuarioId AND product_id = @ProductoId
            """, new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString() }, transaction);
        if (filas > 0)
        {
            await ActualizarFechaAsync(connection, transaction, usuarioId);
        }
        transaction.Commit();
        return true;
    }

    public async Task<bool> VaciarAsync(Guid usuarioId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        var carrito = await LeerAsync(connection, transaction, usuarioId);
        if (carrito == null)
        {
            return false;
        }

        int filas = await connection.ExecuteAsync("DELETE FROM cart_items WHERE user_id = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, transaction);
        if (filas > 0)
        {
            await ActualizarFechaAsync(connection, transaction, usuarioId);
        }
        transaction.Commit();
        return true;
    }

    private static async Task<Carrito?> LeerAsync(SqliteConnection connection, SqliteTransaction transaction, Guid usuarioId)
    {
        string? fecha = await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT updated_at FROM carts WHERE user_id = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, transaction);
        if (fecha == null)
        {
            return null;
        }

        var carrito = new Carrito
        {
            UsuarioId = usuarioId,
            FechaActualizacion = DateTime.Parse(fecha, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
        };
        var filas = await connection.QueryAsync<ItemGuardado>(
            "SELECT product_id AS ProductoId, quantity AS Cantidad FROM cart_items WHERE user_id = @UsuarioId ORDER BY product_id",
            new { UsuarioId = usuarioId.ToString() }, transaction);
        foreach (ItemGuardado fila in filas)
        {
            carrito.Items.Add(new CartItem { ProductoId = Guid.Parse(fila.ProductoId), Cantidad = fila.Cantidad });
        }
        return carrito;
    }

    private static Task ActualizarFechaAsync(SqliteConnection connection, SqliteTransaction transaction, Guid usuarioId)
    {
        return connection.ExecuteAsync("UPDATE carts SET updated_at = @Fecha WHERE user_id = @UsuarioId",
            new { Fecha = FechaActual(), UsuarioId = usuarioId.ToString() }, transaction);
    }

    private static string FechaActual()
    {
        return DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    }

    private class ItemGuardado
    {
        public string ProductoId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
