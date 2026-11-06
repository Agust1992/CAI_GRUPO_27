using System.Text.Json.Serialization;

namespace Products.API.DTOs;

public class OrdenProductoResponse
{
    [JsonRequired]
    public string Estado { get; set; } = string.Empty;
    [JsonRequired]
    public List<OrdenItemResponse> Items { get; set; } = new List<OrdenItemResponse>();
}

public class OrdenItemResponse
{
    [JsonRequired]
    public Guid ProductoId { get; set; }
}
