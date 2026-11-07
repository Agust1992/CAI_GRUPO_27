namespace Users.API.DTOs;

// Datos que la API devuelve cuando el inicio de sesión es correcto.
public class LoginUserResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}