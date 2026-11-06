using Microsoft.AspNetCore.Diagnostics;

namespace Products.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        _logger.LogError(exception, "PRD-005: Error inesperado en Productos");
        await RespuestaError.EscribirAsync(context, 500, "PRD-005",
            "Error interno al procesar el producto.", cancellationToken);
        return true;
    }
}
