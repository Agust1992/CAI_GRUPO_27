namespace Cart.API.Models;

/// <summary>Carrito de un usuario, sin precios ni reservas de stock.</summary>
public class Cart
{
    /// <summary>Identificador del usuario.</summary>
    public Guid UsuarioId { get; set; }
    /// <summary>Productos y cantidades elegidas.</summary>
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    /// <summary>Ultimo cambio del carrito, en UTC.</summary>
    public DateTime FechaActualizacion { get; set; }
}
