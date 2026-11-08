using Microsoft.AspNetCore.Diagnostics;
using Users.API.DTOs;

namespace Users.API.ExceptionHandlers;

// Devuelve una respuesta común cuando ocurre un error inesperado.
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

        httpContext.Items["ErrorCode"] = "USR-006";

        _logger.LogError(
            exception,
            "Error USR-006 en {Endpoint}. CorrelationId: {CorrelationId}",
            httpContext.Request.Path,
            correlationId);

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = 500,
            Detail = "Ocurrió un error inesperado.",
            Instance = httpContext.Request.Path.Value ?? string.Empty,
            ErrorCode = "USR-006",
            ErrorMessage = "Error interno al procesar el usuario.",
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