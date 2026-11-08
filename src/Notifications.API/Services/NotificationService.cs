using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

// Aplica las reglas para crear notificaciones.
public class NotificationService
{
    private readonly NotificationRepository _notificationRepository;
    private readonly UserApiClient _userApiClient;

    public NotificationService(
        NotificationRepository notificationRepository,
        UserApiClient userApiClient)
    {
        _notificationRepository = notificationRepository;
        _userApiClient = userApiClient;
    }

    // Comprueba el usuario y guarda la notificación.
    public async Task<NotificationResponse> CreateAsync(
        CreateNotificationRequest request)
    {
        if (request.UsuarioId is null || request.UsuarioId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "NTF-002",
                "Los datos de la notificación son inválidos.");
        }

        var usuarioExiste =
            await _userApiClient.UserExistsAsync(request.UsuarioId.Value);

        if (!usuarioExiste)
        {
            throw new BusinessRuleException(
                "NTF-001",
                "Usuario no encontrado.");
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId.Value,
            Tipo = request.Tipo,
            Mensaje = request.Mensaje,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        await _notificationRepository.CreateAsync(notification);

        return new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Tipo = notification.Tipo,
            Mensaje = notification.Mensaje,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }

    // Busca las notificaciones de un usuario.
    public async Task<List<NotificationResponse>> GetByUsuarioIdAsync(
        Guid usuarioId)
    {
        var usuarioExiste =
            await _userApiClient.UserExistsAsync(usuarioId);

        if (!usuarioExiste)
        {
            throw new BusinessRuleException(
                "NTF-001",
                "Usuario no encontrado.");
        }

        var notifications =
            await _notificationRepository.GetByUsuarioIdAsync(usuarioId);

        if (notifications.Count == 0)
        {
            throw new BusinessRuleException(
                "NTF-003",
                "No se encontraron notificaciones para el usuario.");
        }

        return notifications.Select(notification => new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Tipo = notification.Tipo,
            Mensaje = notification.Mensaje,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        }).ToList();
    }
}