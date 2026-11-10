namespace Users.API.DTOs;

// Datos que la API devuelve cuando el inicio de sesión es correcto.
/// <summary>
/// Datos públicos devueltos al iniciar sesión correctamente.
/// </summary>
public class LoginUserResponse
{
    /// <summary>
    /// Identificador del usuario autenticado.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Nombre del usuario autenticado.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>
    /// Apellido del usuario autenticado.
    /// </summary>
    public string Apellido { get; set; } = string.Empty;
    /// <summary>
    /// Correo electrónico del usuario autenticado.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}