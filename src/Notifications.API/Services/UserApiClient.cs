using System.Net;

namespace Notifications.API.Services;

// Consulta Users.API para comprobar si existe un usuario.
public class UserApiClient
{
    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> UserExistsAsync(Guid usuarioId)
    {
        var response = await _httpClient.GetAsync(
            $"/api/users/{usuarioId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}