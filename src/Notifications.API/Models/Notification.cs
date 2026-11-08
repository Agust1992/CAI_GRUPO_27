namespace Notifications.API.Models;

/// <summary>
/// Notificación que se guarda en la base de datos.
/// </summary>
public class Notification
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