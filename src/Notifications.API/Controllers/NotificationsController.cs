using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // INICIO DEL POST PARA CREAR UNA NOTIFICACIÓN.
    [HttpPost]
    public async Task<IActionResult> Create(CreateNotificationRequest request)
    {
        var response = await _notificationService.CreateAsync(request);

        return StatusCode(201, response);
    }

    // INICIO DEL GET PARA BUSCAR NOTIFICACIONES POR USUARIO.
    [HttpGet("{usuarioId:guid}")]
    public async Task<IActionResult> GetByUsuarioId(Guid usuarioId)
    {
        var response =
            await _notificationService.GetByUsuarioIdAsync(usuarioId);

        return Ok(response);
    }
}