namespace Notifications.API.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}