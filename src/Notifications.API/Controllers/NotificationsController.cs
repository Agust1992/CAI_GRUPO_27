using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

/// <summary>
/// Permite crear y consultar las notificaciones de los usuarios.
/// </summary>

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]

    // INICIO DEL POST PARA CREAR UNA NOTIFICACIÓN.
    /// <summary>
    /// Registra una notificación para un usuario existente.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> Create(CreateNotificationRequest request)
    {
        var response = await _notificationService.CreateAsync(request);

        return StatusCode(201, response);
    }

    [ProducesResponseType(typeof(List<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]

    // INICIO DEL GET PARA BUSCAR NOTIFICACIONES POR USUARIO.
    /// <summary>
    /// Devuelve las notificaciones de un usuario.
    /// </summary>
    [HttpGet("{usuarioId:guid}")]
    public async Task<IActionResult> GetByUsuarioId(Guid usuarioId)
    {
        var response =
            await _notificationService.GetByUsuarioIdAsync(usuarioId);

        return Ok(response);
    }
}