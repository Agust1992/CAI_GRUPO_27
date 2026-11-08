using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

// Convierte errores de negocio en respuestas HTTP.
public class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(
        ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException error)
        {
            return false;
        }

        var status = error.ErrorCode switch
        {
            "USR-001" => 409,
            "USR-003" => 401,
            "USR-004" or "USR-005" => 403,
            "USR-007" => 404,
            _ => 400
        };

        var title = ReasonPhrases.GetReasonPhrase(status);

        var detail = status switch
        {
            409 => "Ya existe un recurso con esos datos.",
            401 => "Las credenciales no son válidas.",
            403 => "El acceso está prohibido.",
            404 => "El recurso solicitado no fue encontrado.",
            _ => "Los datos del usuario son inválidos."
        };

        var type = status switch
        {
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        var response = new ErrorResponse
        {
            Type = type,
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = error.ErrorCode,
            ErrorMessage = error.Message,
            CorrelationId = context.TraceIdentifier
        };

        context.Items["ErrorCode"] = error.ErrorCode;

        _logger.LogWarning(
            "Error en {Endpoint}. Código {ErrorCode}: {Message}. CorrelationId: {CorrelationId}",
            context.Request.Path,
            error.ErrorCode,
            error.Message,
            context.TraceIdentifier);

        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(
            response,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}