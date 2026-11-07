using System.Text.Json.Serialization;

namespace Products.API.DTOs;

/// <summary>Datos del cliente para crear un producto; no permite asignar Id ni fecha.</summary>
/// <param name="Nombre">Obligatorio, maximo 100 caracteres.</param>
/// <param name="Descripcion">Opcional, maximo 500 caracteres.</param>
/// <param name="Precio">Mayor a cero.</param>
/// <param name="Stock">Cero o mas.</param>
/// <param name="Categoria">Obligatoria, sin valores restringidos.</param>
public record CrearProductoRequest(
    string Nombre,
    string? Descripcion,
    [property: JsonRequired] decimal Precio,
    [property: JsonRequired] int Stock,
    string? Categoria
);
