using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

// Datos que se reciben para crear una notificación.
public class CreateNotificationRequest
{
    [Required]
    public Guid? UsuarioId { get; set; }

    [Required]
    [RegularExpression(
        "^(Email|Push|SMS)$",
        ErrorMessage = "El tipo debe ser Email, Push o SMS.")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Mensaje { get; set; } = string.Empty;
}