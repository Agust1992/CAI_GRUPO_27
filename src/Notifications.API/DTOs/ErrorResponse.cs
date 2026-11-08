namespace Notifications.API.DTOs;

/// <summary>
/// Datos que la API devuelve cuando ocurre un error.
/// </summary>

public class ErrorResponse
{
    /// <summary>Enlace que describe el tipo de error.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Nombre breve del error HTTP.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Código de estado HTTP del error.</summary>
    public int Status { get; set; }

    /// <summary>Explicación general del error.</summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>Ruta que recibió el pedido.</summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>Código propio del error, como NTF-001.</summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>Mensaje concreto del catálogo de errores.</summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Identificador usado para seguir el pedido en los logs.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}