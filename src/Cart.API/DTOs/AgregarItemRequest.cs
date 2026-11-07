using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cart.API.DTOs;

/// <summary>Datos para agregar unidades de un producto.</summary>
public class AgregarItemRequest
{
    /// <summary>Producto que se consulta en Products.API.</summary>
    [JsonRequired]
    public Guid ProductoId { get; set; }

    /// <summary>Cantidad que se suma a la existente.</summary>
    /// <example>2</example>
    [JsonRequired]
    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}
