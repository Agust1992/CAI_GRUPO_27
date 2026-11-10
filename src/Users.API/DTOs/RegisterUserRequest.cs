using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

// Datos que se envían para registrar un usuario.
/// <summary>
/// Datos requeridos para registrar un usuario.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required] // El nombre es obligatorio.
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    [Required] // El apellido es obligatorio.
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico válido y obligatorio.
    /// </summary>
    [Required] // El email no puede faltar.
    [EmailAddress] // El email debe tener un formato válido.
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña obligatoria para crear la cuenta.
    /// </summary>
    [Required] // La contraseña no puede faltar.
    public string Password { get; set; } = string.Empty;
}