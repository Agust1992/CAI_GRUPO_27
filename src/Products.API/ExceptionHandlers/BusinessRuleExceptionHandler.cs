using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

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

        _logger.LogWarning("{ErrorCode}: {Mensaje}", error.ErrorCode, error.Message);
        await RespuestaError.EscribirAsync(context, 409, error.ErrorCode, error.Message, cancellationToken);
        return true;
    }
}
