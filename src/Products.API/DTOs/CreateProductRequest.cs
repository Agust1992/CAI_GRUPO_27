namespace Products.API.DTOs;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? Category
);
