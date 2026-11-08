namespace Notifications.API.DTOs;

/// <summary>
/// Datos que la API devuelve sobre una notificación.
/// </summary>
/// 

/// <example>
/// {
///   "id": "26c04593-3e3a-438f-a752-6ca1dc6ad044",
///   "usuarioId": "e7ed291e-841e-4592-ab9e-4b1614550aea",
///   "tipo": "Email",
///   "mensaje": "Su orden fue confirmada.",
///   "estado": "Enviada",
///   "fechaEnvio": "2026-11-08T04:31:19.1996133Z"
/// }
/// </example>
/// 

public class NotificationResponse
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del usuario destinatario.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Medio usado para enviar la notificación: Email, Push o SMS.
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Texto enviado al usuario.
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Resultado del envío, por ejemplo: Enviada.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora en que se registró el envío.
    /// </summary>
    public DateTime FechaEnvio { get; set; }
}