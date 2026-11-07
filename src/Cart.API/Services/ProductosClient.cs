using System.Net;
using Cart.API.DTOs;
using Cart.API.Exceptions;

namespace Cart.API.Services;

public class ProductosClient
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _contextAccessor;

    public ProductosClient(HttpClient client, IHttpContextAccessor contextAccessor)
    {
        _client = client;
        _contextAccessor = contextAccessor;
    }

    public async Task<ProductoStockResponse> ObtenerAsync(Guid productoId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/products/{productoId}");
        var context = _contextAccessor.HttpContext;
        if (context != null)
        {
            request.Headers.Add("X-Correlation-Id", context.TraceIdentifier);
        }

        using var response = await _client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Productos respondio con estado {(int)response.StatusCode}.");
        }

        var producto = await response.Content.ReadFromJsonAsync<ProductoStockResponse>();
        if (producto == null || producto.Id != productoId || producto.Stock < 0)
        {
            throw new InvalidOperationException("La respuesta de Productos no es valida.");
        }
        return producto;
    }
}
