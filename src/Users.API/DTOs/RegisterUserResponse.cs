namespace Users.API.DTOs;

// Datos que la API devuelve cuando registra un usuario.
/// <summary>
/// Datos públicos devueltos al registrar un usuario.
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// Identificador asignado al usuario.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Nombre del usuario registrado.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>
    /// Apellido del usuario registrado.
    /// </summary>
    public string Apellido { get; set; } = string.Empty;
    /// <summary>
    /// Correo electrónico del usuario registrado.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Fecha y hora en que se registró la cuenta.
    /// </summary>
    public DateTime FechaRegistro { get; set; }
    /// <summary>
    /// Indica si la cuenta está habilitada.
    /// </summary>
    public bool Activo { get; set; }
}