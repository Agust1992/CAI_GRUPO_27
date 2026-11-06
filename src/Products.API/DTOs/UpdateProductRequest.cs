using System.Text.Json.Serialization;

namespace Products.API.DTOs;

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    [property: JsonRequired] int Stock,
    string? Category
);
