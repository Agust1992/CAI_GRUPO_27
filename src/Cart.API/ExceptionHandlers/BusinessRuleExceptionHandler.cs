using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException error)
        {
            return false;
        }

        _logger.LogWarning("{errorCode}: {Mensaje}", error.ErrorCode, error.Message);
        await RespuestaError.EscribirAsync(context, 422, error.ErrorCode, error.Message, cancellationToken);
        return true;
    }
}
