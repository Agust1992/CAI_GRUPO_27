using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Users.API.DTOs;

namespace Users.API.ExceptionHandlers;

// Sustituye respuestas 415 creadas por MVC por el formato de error de Users.
public sealed class UnsupportedMediaTypeResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not IStatusCodeActionResult
            { StatusCode: StatusCodes.Status415UnsupportedMediaType })
        {
            return;
        }

        var httpContext = context.HttpContext;
        const string errorCode = "USR-009";
        const string errorMessage = "Tipo de contenido no soportado.";
        httpContext.Items["ErrorCode"] = errorCode;

        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.13",
            Title = "Unsupported Media Type",
            Status = StatusCodes.Status415UnsupportedMediaType,
            Detail = "Enviá los datos con Content-Type application/json.",
            Instance = httpContext.Request.Path.Value ?? string.Empty,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CorrelationId = httpContext.TraceIdentifier
        };

        var result = new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status415UnsupportedMediaType
        };
        result.ContentTypes.Add("application/problem+json");
        context.Result = result;

        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Users.API.HttpErrors");
        logger.LogWarning(
            "HTTP {StatusCode} en {Endpoint}. Código {ErrorCode}: {Message}. CorrelationId: {CorrelationId}",
            StatusCodes.Status415UnsupportedMediaType,
            httpContext.Request.Path,
            errorCode,
            errorMessage,
            httpContext.TraceIdentifier);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}