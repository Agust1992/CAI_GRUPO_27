namespace Users.API.Models;

// Datos que la API guarda sobre un usuario.
/// <summary>
/// Representa una cuenta de usuario almacenada por el servicio.
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; } // Identificador que genera la API.

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Apellido { get; set; } = string.Empty;
    /// <summary>
    /// Dirección de correo utilizada para identificar la cuenta.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña; nunca se devuelve al cliente.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty; // Hash; nunca se devuelve al cliente.

    /// <summary>
    /// Fecha y hora de creación de la cuenta.
    /// </summary>
    public DateTime FechaRegistro { get; set; } // Fecha que asigna la API.
    /// <summary>
    /// Indica si la cuenta está habilitada para iniciar sesión.
    /// </summary>
    public bool Activo { get; set; } // Indica si puede iniciar sesión.
    /// <summary>
    /// Cantidad de intentos de inicio de sesión fallidos.
    /// </summary>
    public int IntentosFallidos { get; set; } // Cuenta los intentos de login incorrectos.
}