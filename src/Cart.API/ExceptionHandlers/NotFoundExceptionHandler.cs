using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException error)
        {
            return false;
        }

        _logger.LogWarning("{errorCode}: {Mensaje}", error.ErrorCode, error.Message);
        await RespuestaError.EscribirAsync(context, 404, error.ErrorCode, error.Message, cancellationToken);
        return true;
    }
}
