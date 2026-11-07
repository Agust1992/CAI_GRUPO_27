namespace Cart.API.Models;

/// <summary>Producto incluido en un carrito.</summary>
public class CartItem
{
    /// <summary>Identificador de Products.API.</summary>
    public Guid ProductoId { get; set; }
    /// <summary>Unidades solicitadas.</summary>
    public int Cantidad { get; set; }
}
