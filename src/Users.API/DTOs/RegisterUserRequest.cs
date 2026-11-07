using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

// Datos que se envían para registrar un usuario.
public class RegisterUserRequest
{
    [Required] // El nombre es obligatorio.
    public string Nombre { get; set; } = string.Empty;

    [Required] // El apellido es obligatorio.
    public string Apellido { get; set; } = string.Empty;

    [Required] // El email no puede faltar.
    [EmailAddress] // El email debe tener un formato válido.
    public string Email { get; set; } = string.Empty;

    [Required] // La contraseña no puede faltar.
    public string Password { get; set; } = string.Empty;
}