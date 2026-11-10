namespace Notifications.API.Exceptions;

/// <summary>
/// Representa una búsqueda que no encontró el recurso solicitado.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Código funcional asociado al recurso faltante.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Crea la excepción con el código y el mensaje del catálogo.
    /// </summary>
    public NotFoundException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
