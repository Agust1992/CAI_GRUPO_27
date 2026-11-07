using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cart.API.DTOs;

/// <summary>Nueva cantidad total de un producto del carrito.</summary>
public class ActualizarCantidadRequest
{
    /// <summary>Reemplaza la cantidad anterior; no se suma.</summary>
    /// <example>4</example>
    [JsonRequired]
    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}
