namespace Products.API.Models;

public record Product
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string? Category { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public string? UpdatedAt { get; init; }
}
