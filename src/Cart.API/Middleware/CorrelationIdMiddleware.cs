using Serilog.Context;

namespace Cart.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers["X-Correlation-Id"].ToString();
        if (!EsValido(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Endpoint", context.Request.Path.Value))
        {
            _logger.LogInformation("Inicio de {RequestMethod} {Endpoint}",
                context.Request.Method, context.Request.Path.Value);
            await _next(context);
        }
    }

    private static bool EsValido(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > 100)
        {
            return false;
        }
        foreach (char caracter in valor)
        {
            if (caracter < 33 || caracter > 126)
            {
                return false;
            }
        }
        return true;
    }
}
