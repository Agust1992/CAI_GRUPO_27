namespace Users.API.Exceptions;

// Guarda el código y el mensaje de un error de negocio.
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}