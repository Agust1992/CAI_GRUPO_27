using Microsoft.AspNetCore.Mvc;

namespace Cart.API.DTOs;

/// <summary>Error HTTP con el codigo del catalogo y el identificador de la solicitud.</summary>
public class ErrorResponse : ProblemDetails
{
    /// <summary>Codigo CRT del catalogo.</summary>
    public string ErrorCode { get; set; } = string.Empty;
    /// <summary>Mensaje que puede mostrarse al cliente.</summary>
    public string ErrorMessage { get; set; } = string.Empty;
    /// <summary>Relaciona la respuesta con los logs y llamadas HTTP.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}
