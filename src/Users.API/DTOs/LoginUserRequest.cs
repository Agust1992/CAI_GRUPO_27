using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

// Datos que se envían para iniciar sesión.
public class LoginUserRequest
{
    [Required] // El email es obligatorio.
    [EmailAddress] // Debe tener un formato de email válido.
    public string Email { get; set; } = string.Empty;

    [Required] // La contraseña es obligatoria.
    public string Password { get; set; } = string.Empty;
}