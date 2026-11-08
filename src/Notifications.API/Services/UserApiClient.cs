using System.Net;
using Microsoft.AspNetCore.Http;

namespace Notifications.API.Services;

// Consulta Users.API y le reenvía el identificador del pedido.
public class UserApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> UserExistsAsync(Guid usuarioId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/users/{usuarioId}");

        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation(
                "X-Correlation-Id",
                correlationId);
        }

        using var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}