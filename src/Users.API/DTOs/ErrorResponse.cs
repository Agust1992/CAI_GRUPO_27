namespace Users.API.DTOs;

// Forma común de las respuestas de error.
/// <summary>
/// Formato común de las respuestas de error de la API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// URI que identifica el tipo de error.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Descripción breve del error.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Código de estado HTTP.
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// Descripción del error en esta solicitud.
    /// </summary>
    public string Detail { get; set; } = string.Empty;
    /// <summary>
    /// Ruta de la solicitud que produjo el error.
    /// </summary>
    public string Instance { get; set; } = string.Empty;
    /// <summary>
    /// Código funcional del error.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    /// <summary>
    /// Mensaje funcional del error.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    /// <summary>
    /// Identificador para seguir la solicitud en los logs.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
}