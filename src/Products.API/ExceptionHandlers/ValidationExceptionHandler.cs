using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException error)
        {
            return false;
        }

        _logger.LogWarning("{errorCode}: {Mensaje}", error.ErrorCode, error.Message);
        await RespuestaError.EscribirAsync(context, 400, error.ErrorCode, error.Message, cancellationToken);
        return true;
    }
}
