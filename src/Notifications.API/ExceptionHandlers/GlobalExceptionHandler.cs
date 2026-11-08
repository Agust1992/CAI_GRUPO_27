using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;

namespace Notifications.API.ExceptionHandlers;

// Devuelve una respuesta común ante un error inesperado.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;

        httpContext.Items["ErrorCode"] = "NTF-004";

        _logger.LogError(
            exception,
            "Error NTF-004 en {Endpoint}. CorrelationId: {CorrelationId}",
            httpContext.Request.Path,
            correlationId);

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = 500,
            Detail = "Ocurrió un error inesperado.",
            Instance = httpContext.Request.Path,
            ErrorCode = "NTF-004",
            ErrorMessage = "Error interno al procesar la notificación.",
            CorrelationId = correlationId
        };

        httpContext.Response.StatusCode = 500;

        await httpContext.Response.WriteAsJsonAsync(
            errorResponse,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}