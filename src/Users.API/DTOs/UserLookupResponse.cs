namespace Users.API.DTOs;

// Datos públicos que puede consultar otro servicio.
public class UserLookupResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}