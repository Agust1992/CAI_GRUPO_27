using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

// Convierte errores esperados en respuestas HTTP.
public class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(
        ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException businessException)
        {
            return false;
        }

        var statusCode = 400;

        if (businessException.ErrorCode == "NTF-001" ||
            businessException.ErrorCode == "NTF-003")
        {
            statusCode = 404;
        }

        var title = statusCode == 404 ? "Not Found" : "Bad Request";
        var type = statusCode == 404
            ? "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            : "https://tools.ietf.org/html/rfc7231#section-6.5.1";

        _logger.LogWarning(
            "Error {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
            businessException.ErrorCode,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        var errorResponse = new ErrorResponse
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = businessException.Message,
            Instance = httpContext.Request.Path,
            ErrorCode = businessException.ErrorCode,
            ErrorMessage = businessException.Message,
            CorrelationId = httpContext.TraceIdentifier
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            errorResponse,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}