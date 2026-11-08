using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

/// <summary>
/// Datos necesarios para registrar una notificación.
/// </summary>
/// <example>
/// {
///   "usuarioId": "e7ed291e-841e-4592-ab9e-4b1614550aea",
///   "tipo": "Email",
///   "mensaje": "Su orden fue confirmada."
/// }
/// </example>
public class CreateNotificationRequest
{
    /// <summary>
    /// Identificador del usuario que recibirá la notificación.
    /// </summary>
    [Required]
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Medio de envío: Email, Push o SMS.
    /// </summary>
    [Required]
    [RegularExpression(
        "^(Email|Push|SMS)$",
        ErrorMessage = "El tipo debe ser Email, Push o SMS.")]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Texto que recibirá el usuario.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Mensaje { get; set; } = string.Empty;
}