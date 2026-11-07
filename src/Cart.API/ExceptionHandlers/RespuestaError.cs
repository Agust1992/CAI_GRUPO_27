using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Cart.API.DTOs;

namespace Cart.API.ExceptionHandlers;

public static class RespuestaError
{
    public static ErrorResponse CrearProblema(HttpContext context, int estado, string codigo, string mensaje)
    {
        string tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        if (estado == 404)
        {
            tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
        }
        if (estado == 409)
        {
            tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
        }
        if (estado == 405)
        {
            tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.5";
        }
        if (estado == 415)
        {
            tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.13";
        }
        if (estado == 422)
        {
            tipo = "https://tools.ietf.org/html/rfc4918#section-11.2";
        }
        if (estado == 500)
        {
            tipo = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        }

        return new ErrorResponse
        {
            Type = tipo,
            Title = ReasonPhrases.GetReasonPhrase(estado),
            Status = estado,
            Detail = mensaje,
            Instance = context.Request.Path,
            ErrorCode = codigo,
            ErrorMessage = mensaje,
            CorrelationId = context.TraceIdentifier
        };
    }

    public static ErrorResponse CrearErrorHttp(HttpContext context, int estado)
    {
        string codigo = "CRT-004";
        string mensaje = "La solicitud no es valida para Carrito.";
        if (estado == 404)
        {
            codigo = "CRT-001";
            mensaje = "Carrito no encontrado.";
        }
        if (estado >= 500)
        {
            codigo = "CRT-005";
            mensaje = "Error interno al procesar el carrito.";
        }
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Cart.API.HttpErrors");
        if (estado >= 500)
        {
            logger.LogError("{errorCode}: HTTP {StatusCode}", codigo, estado);
        }
        else
        {
            logger.LogWarning("{errorCode}: HTTP {StatusCode}", codigo, estado);
        }
        return CrearProblema(context, estado, codigo, mensaje);
    }

    public static async Task EscribirAsync(HttpContext context, int estado, string codigo,
        string mensaje, CancellationToken cancellationToken)
    {
        var problema = CrearProblema(context, estado, codigo, mensaje);
        context.Response.StatusCode = estado;
        await context.Response.WriteAsJsonAsync(problema, options: null,
            contentType: "application/problem+json", cancellationToken: cancellationToken);
    }
}
