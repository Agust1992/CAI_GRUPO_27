using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

/// <summary>
/// Convierte búsquedas sin resultado en respuestas 404 con el formato común.
/// </summary>
public sealed class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    /// <summary>
    /// Crea el manejador de recursos no encontrados.
    /// </summary>
    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFound)
        {
            return false;
        }

        context.Items["ErrorCode"] = notFound.ErrorCode;
        _logger.LogWarning(
            "Error 404 en {Endpoint}. Código {ErrorCode}: {Message}. CorrelationId: {CorrelationId}",
            context.Request.Path,
            notFound.ErrorCode,
            notFound.Message,
            context.TraceIdentifier);

        var errorMessage = notFound.ErrorCode switch
        {
            "NTF-001" => "Usuario no encontrado.",
            "NTF-003" => "No se encontraron notificaciones para el usuario.",
            _ => notFound.Message
        };

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = "El recurso solicitado no fue encontrado.",
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = notFound.ErrorCode,
            ErrorMessage = errorMessage,
            CorrelationId = context.TraceIdentifier
        }, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }
}
