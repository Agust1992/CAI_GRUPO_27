using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Models;
using Products.API.Services;

namespace Products.API.Controllers;

[ApiController]
[Route("api/products")]
[Tags("Products")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public class ProductosController : ControllerBase
{
    private readonly ProductoService _service;

    public ProductosController(ProductoService service)
    {
        _service = service;
    }

    /// <summary>Lista productos y permite combinar filtros.</summary>
    /// <param name="nombre">Parte del nombre, sin distinguir mayusculas.</param>
    /// <param name="categoria">Categoria completa, sin distinguir mayusculas.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? nombre, [FromQuery] string? categoria)
    {
        var productos = await _service.ListarAsync(nombre, categoria);
        return Ok(productos);
    }

    /// <summary>Busca un producto por su Guid.</summary>
    /// <response code="404">PRD-001: el producto no existe.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar(Guid id)
    {
        var producto = await _service.BuscarAsync(id);
        return Ok(producto);
    }

    /// <summary>Crea un producto y asigna su Id y fecha de creacion.</summary>
    /// <response code="409">PRD-003: nombre y categoria duplicados.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(CrearProductoRequest request)
    {
        var producto = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(Buscar), new { id = producto.Id }, producto);
    }

    /// <summary>Actualiza el producto sin modificar su Id ni fecha de creacion.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, ActualizarProductoRequest request)
    {
        var producto = await _service.ActualizarAsync(id, request);
        return Ok(producto);
    }

    /// <summary>Elimina un producto si no tiene ordenes Pendiente o Confirmada.</summary>
    /// <response code="409">PRD-004: hay ordenes activas.</response>
    /// <response code="500">PRD-005: no se pudo comprobar Orders; el producto se conserva.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
