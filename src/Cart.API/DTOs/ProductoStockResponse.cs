using System.Text.Json.Serialization;

namespace Cart.API.DTOs;

public class ProductoStockResponse
{
    [JsonRequired]
    public Guid Id { get; set; }
    [JsonRequired]
    public int Stock { get; set; }
}
