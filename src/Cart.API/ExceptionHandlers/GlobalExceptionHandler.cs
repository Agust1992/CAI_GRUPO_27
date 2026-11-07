using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers;

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

        _logger.LogError(exception, "{errorCode}: Error inesperado en Carrito", "CRT-005");
        await RespuestaError.EscribirAsync(context, 500, "CRT-005",
            "Error interno al procesar el carrito.", cancellationToken);
        return true;
    }
}
