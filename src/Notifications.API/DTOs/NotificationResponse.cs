namespace Notifications.API.DTOs;

// Datos públicos que devuelve la API sobre una notificación.
public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}