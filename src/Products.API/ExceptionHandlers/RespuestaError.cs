using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Products.API.ExceptionHandlers;

public static class RespuestaError
{
    public static ProblemDetails CrearProblema(HttpContext context, int estado, string codigo, string mensaje)
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
        if (estado == 500)
        {
            tipo = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        }

        var problema = new ProblemDetails
        {
            Type = tipo,
            Title = ReasonPhrases.GetReasonPhrase(estado),
            Status = estado,
            Detail = mensaje,
            Instance = context.Request.Path
        };
        problema.Extensions["errorCode"] = codigo;
        problema.Extensions["errorMessage"] = mensaje;
        problema.Extensions["correlationId"] = context.TraceIdentifier;
        return problema;
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
