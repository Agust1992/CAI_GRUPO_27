using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

/// <summary>
/// Convierte validaciones fallidas en respuestas 400 con el formato común.
/// </summary>
public sealed class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    /// <summary>
    /// Crea el manejador de validaciones fallidas.
    /// </summary>
    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation)
        {
            return false;
        }

        context.Items["ErrorCode"] = validation.ErrorCode;
        _logger.LogWarning(
            "Error 400 en {Endpoint}. Código {ErrorCode}: {Message}. CorrelationId: {CorrelationId}",
            context.Request.Path,
            validation.ErrorCode,
            validation.Message,
            context.TraceIdentifier);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = validation.Message,
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = validation.ErrorCode,
            ErrorMessage = validation.Message,
            CorrelationId = context.TraceIdentifier
        }, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }
}
