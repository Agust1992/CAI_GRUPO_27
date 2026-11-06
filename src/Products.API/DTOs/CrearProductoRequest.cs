using System.Text.Json.Serialization;

namespace Products.API.DTOs;

public record CrearProductoRequest(
    string Nombre,
    string? Descripcion,
    [property: JsonRequired] decimal Precio,
    [property: JsonRequired] int Stock,
    string? Categoria
);
