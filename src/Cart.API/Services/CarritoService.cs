using Cart.API.Data;
using Cart.API.Exceptions;
using Cart.API.Models;
using Carrito = Cart.API.Models.Cart;

namespace Cart.API.Services;

public class CarritoService
{
    private readonly CarritoRepository _repository;
    private readonly ProductosClient _productos;

    public CarritoService(CarritoRepository repository, ProductosClient productos)
    {
        _repository = repository;
        _productos = productos;
    }

    public async Task<Carrito> ObtenerAsync(Guid usuarioId)
    {
        ValidarId(usuarioId);
        var carrito = await _repository.ObtenerAsync(usuarioId);
        if (carrito == null)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }
        return carrito;
    }

    public async Task<Carrito> AgregarAsync(Guid usuarioId, Guid productoId, int cantidad)
    {
        ValidarId(usuarioId);
        ValidarId(productoId);
        ValidarCantidad(cantidad);
        var producto = await _productos.ObtenerAsync(productoId);
        var carrito = await _repository.AgregarAsync(usuarioId, productoId, cantidad, producto.Stock);
        if (carrito == null)
        {
            throw new BusinessRuleException("CRT-003", "Stock insuficiente para agregar al carrito.");
        }
        return carrito;
    }

    public async Task<Carrito> ActualizarAsync(Guid usuarioId, Guid productoId, int cantidad)
    {
        ValidarId(productoId);
        ValidarCantidad(cantidad);
        var carrito = await ObtenerAsync(usuarioId);
        bool encontrado = false;
        foreach (CartItem item in carrito.Items)
        {
            if (item.ProductoId == productoId)
            {
                encontrado = true;
                break;
            }
        }
        if (!encontrado)
        {
            throw new NotFoundException("CRT-002", "El producto no se encuentra en el carrito.");
        }

        var producto = await _productos.ObtenerAsync(productoId);
        if (cantidad > producto.Stock)
        {
            throw new BusinessRuleException("CRT-003", "Stock insuficiente para agregar al carrito.");
        }
        var actualizado = await _repository.ActualizarCantidadAsync(usuarioId, productoId, cantidad);
        if (actualizado == null)
        {
            throw new NotFoundException("CRT-002", "El producto no se encuentra en el carrito.");
        }
        return actualizado;
    }

    public async Task EliminarItemAsync(Guid usuarioId, Guid productoId)
    {
        ValidarId(usuarioId);
        ValidarId(productoId);
        bool encontrado = await _repository.EliminarItemAsync(usuarioId, productoId);
        if (!encontrado)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }
    }

    public async Task VaciarAsync(Guid usuarioId)
    {
        ValidarId(usuarioId);
        bool encontrado = await _repository.VaciarAsync(usuarioId);
        if (!encontrado)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }
    }

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ValidationException("CRT-004", "Cantidad invalida.");
        }
    }

    private static void ValidarId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException("CRT-004", "El identificador no es valido.");
        }
    }
}
