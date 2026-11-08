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

    // INICIO DEL POST PARA CREAR UNA NOTIFICACIÓN.
    /// <summary>
    /// Registra una notificación para un usuario existente.
    /// </summary>
    /// <param name="request">Datos de la notificación que se va a registrar.</param>
    /// <response code="201">La notificación se registró correctamente.</response>
    /// <response code="400">Los datos recibidos son inválidos (NTF-002).</response>
    /// <response code="404">El usuario destinatario no existe (NTF-001).</response>
    /// <response code="500">Ocurrió un error interno (NTF-004).</response>
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/problem+json")]
    [HttpPost("send")]
    public async Task<IActionResult> Create(CreateNotificationRequest request)
    {
        var response = await _notificationService.CreateAsync(request);

        return StatusCode(201, response);
    }

    // INICIO DEL GET PARA BUSCAR NOTIFICACIONES POR USUARIO.
    /// <summary>
    /// Devuelve las notificaciones de un usuario.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario.</param>
    /// <response code="200">Se devolvió la lista de notificaciones.</response>
    /// <response code="404">El usuario no existe o no tiene notificaciones (NTF-001, NTF-003).</response>
    /// <response code="500">Ocurrió un error interno (NTF-004).</response>
    [ProducesResponseType(typeof(List<NotificationResponse>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/problem+json")]
    [HttpGet("{usuarioId:guid}")]
    public async Task<IActionResult> GetByUsuarioId(Guid usuarioId)
    {
        var response =
            await _notificationService.GetByUsuarioIdAsync(usuarioId);

        return Ok(response);
    }
}