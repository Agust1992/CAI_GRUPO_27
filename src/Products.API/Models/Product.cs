namespace Products.API.Models;

/// <summary>Producto del catalogo.</summary>
public record Product
{
    /// <summary>Guid asignado por el servidor.</summary>
    public Guid Id { get; init; }
    /// <summary>Nombre obligatorio, hasta 100 caracteres.</summary>
    public string Nombre { get; init; } = string.Empty;
    /// <summary>Descripcion opcional, hasta 500 caracteres.</summary>
    public string? Descripcion { get; init; }
    /// <summary>Precio decimal mayor a cero.</summary>
    public decimal Precio { get; init; }
    /// <summary>Unidades disponibles, cero o mas.</summary>
    public int Stock { get; init; }
    /// <summary>Categoria obligatoria, sin lista cerrada de valores.</summary>
    public string Categoria { get; init; } = string.Empty;
    /// <summary>Fecha UTC asignada al crear y conservada al modificar.</summary>
    public DateTime FechaCreacion { get; init; }
}
