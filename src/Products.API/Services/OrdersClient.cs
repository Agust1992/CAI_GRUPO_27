using Products.API.DTOs;

namespace Products.API.Services;

public class OrdersClient
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _contextAccessor;

    public OrdersClient(HttpClient client, IHttpContextAccessor contextAccessor)
    {
        _client = client;
        _contextAccessor = contextAccessor;
    }

    public async Task<bool> TieneOrdenActivaAsync(Guid productoId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/orders");
        var context = _contextAccessor.HttpContext;
        if (context != null)
        {
            request.Headers.Add("X-Correlation-Id", context.TraceIdentifier);
        }
        using var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Orders respondio con estado {(int)response.StatusCode}.");
        }
        var ordenes = await response.Content.ReadFromJsonAsync<List<OrdenProductoResponse>>();
        if (ordenes == null)
        {
            throw new InvalidOperationException("La respuesta de Orders no es valida.");
        }
        foreach (OrdenProductoResponse orden in ordenes)
        {
            if (orden == null || orden.Items == null ||
                (orden.Estado != "Pendiente" && orden.Estado != "Confirmada" &&
                 orden.Estado != "Enviada" && orden.Estado != "Entregada" && orden.Estado != "Cancelada"))
            {
                throw new InvalidOperationException("La respuesta de Orders no es valida.");
            }
            foreach (OrdenItemResponse item in orden.Items)
            {
                if (item == null || item.ProductoId == Guid.Empty)
                {
                    throw new InvalidOperationException("La respuesta de Orders no es valida.");
                }
                if (item.ProductoId == productoId &&
                    (orden.Estado == "Pendiente" || orden.Estado == "Confirmada"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
