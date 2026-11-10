using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

// Datos que se envían para iniciar sesión.
/// <summary>
/// Credenciales requeridas para iniciar sesión.
/// </summary>
public class LoginUserRequest
{
    /// <summary>
    /// Correo electrónico asociado a la cuenta.
    /// </summary>
    [Required] // El email es obligatorio.
    [EmailAddress] // Debe tener un formato de email válido.
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña que se va a verificar.
    /// </summary>
    [Required] // La contraseña es obligatoria.
    public string Password { get; set; } = string.Empty;
}