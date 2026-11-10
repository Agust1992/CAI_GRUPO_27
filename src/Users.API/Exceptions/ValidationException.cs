namespace Users.API.Exceptions;

/// <summary>
/// Representa datos de entrada que no cumplen las validaciones de la API.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Código funcional asociado a la validación fallida.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Crea la excepción con el código y el mensaje del catálogo.
    /// </summary>
    public ValidationException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
