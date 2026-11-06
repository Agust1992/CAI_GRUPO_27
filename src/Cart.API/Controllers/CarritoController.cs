using Cart.API.DTOs;
using Cart.API.Services;
using Microsoft.AspNetCore.Mvc;
using Carrito = Cart.API.Models.Cart;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/cart/{userId}")]
[Tags("Cart")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public class CarritoController : ControllerBase
{
    private readonly CarritoService _service;

    public CarritoController(CarritoService service)
    {
        _service = service;
    }

    /// <summary>Consulta el carrito de un usuario.</summary>
    /// <response code="404">CRT-001: el usuario todavia no tiene carrito.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Carrito), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid userId)
    {
        return Ok(await _service.ObtenerAsync(userId));
    }

    /// <summary>Agrega unidades de un producto. Si ya existe, suma la cantidad.</summary>
    /// <response code="400">CRT-004: cantidad o datos invalidos.</response>
    /// <response code="404">CRT-002: producto no encontrado.</response>
    /// <response code="422">CRT-003: la cantidad total supera el stock.</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(Carrito), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Agregar(Guid userId, AgregarItemRequest request)
    {
        return Ok(await _service.AgregarAsync(userId, request.ProductoId, request.Cantidad));
    }

    /// <summary>Reemplaza la cantidad de un producto que ya esta en el carrito.</summary>
    /// <response code="400">CRT-004: cantidad o datos invalidos.</response>
    /// <response code="404">CRT-001: carrito inexistente; CRT-002: producto inexistente.</response>
    /// <response code="422">CRT-003: la cantidad supera el stock.</response>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(Carrito), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Actualizar(Guid userId, Guid productId, ActualizarCantidadRequest request)
    {
        return Ok(await _service.ActualizarAsync(userId, productId, request.Cantidad));
    }

    /// <summary>Quita un producto del carrito. Si el item ya no esta, no cambia nada.</summary>
    /// <response code="404">CRT-001: carrito no encontrado.</response>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarItem(Guid userId, Guid productId)
    {
        await _service.EliminarItemAsync(userId, productId);
        return NoContent();
    }

    /// <summary>Vacia los items y conserva el carrito para el usuario.</summary>
    /// <response code="404">CRT-001: carrito no encontrado.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Vaciar(Guid userId)
    {
        await _service.VaciarAsync(userId);
        return NoContent();
    }
}
