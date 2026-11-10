namespace Users.API.DTOs;

// Datos públicos que puede consultar otro servicio.
/// <summary>
/// Datos públicos de un usuario consultado por identificador.
/// </summary>
public class UserLookupResponse
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Apellido { get; set; } = string.Empty;
    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}