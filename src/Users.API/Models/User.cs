namespace Users.API.Models;

// Datos que la API guarda sobre un usuario.
public class User
{
    public Guid Id { get; set; } // Identificador que genera la API.

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty; // Hash; nunca se devuelve al cliente.

    public DateTime FechaRegistro { get; set; } // Fecha que asigna la API.
    public bool Activo { get; set; } // Indica si puede iniciar sesión.
    public int IntentosFallidos { get; set; } // Cuenta los intentos de login incorrectos.
}