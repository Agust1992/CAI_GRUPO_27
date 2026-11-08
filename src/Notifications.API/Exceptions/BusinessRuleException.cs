namespace Notifications.API.Exceptions;

// Representa un error esperado de las reglas del negocio.
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}