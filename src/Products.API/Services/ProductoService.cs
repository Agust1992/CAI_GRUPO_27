using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductoService
{
    private readonly ProductRepository _repository;

    public ProductoService(ProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Product>> ListarAsync(string? nombre, string? categoria)
    {
        var productos = await _repository.GetAllAsync();
        List<Product> resultado = new List<Product>();

        foreach (Product producto in productos)
        {
            if (!string.IsNullOrWhiteSpace(nombre) &&
                !producto.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(categoria) &&
                !string.Equals(producto.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            resultado.Add(producto);
        }

        return resultado;
    }

    public async Task<Product> BuscarAsync(Guid id)
    {
        var producto = await _repository.GetByIdAsync(id);
        if (producto == null)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        return producto;
    }

    public async Task<Product> CrearAsync(CrearProductoRequest request)
    {
        ValidarProducto(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria);

        var producto = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria!,
            FechaCreacion = DateTime.UtcNow
        };

        bool creado = await _repository.CreateAsync(producto);
        if (!creado)
        {
            throw new BusinessRuleException("PRD-003",
                "Ya existe un producto con ese nombre en la categoria.");
        }

        return producto;
    }

    public async Task<Product> ActualizarAsync(Guid id, ActualizarProductoRequest request)
    {
        ValidarProducto(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria);

        bool actualizado = await _repository.UpdateAsync(id, request);
        if (!actualizado)
        {
            await BuscarAsync(id);
            throw new BusinessRuleException("PRD-003",
                "Ya existe un producto con ese nombre en la categoria.");
        }

        return await BuscarAsync(id);
    }

    public async Task EliminarAsync(Guid id)
    {
        bool eliminado = await _repository.DeleteAsync(id);
        if (!eliminado)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }
    }

    private static void ValidarProducto(string? nombre, string? descripcion,
        decimal precio, int stock, string? categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ValidationException("PRD-002", "El nombre del producto es obligatorio.");
        }

        if (nombre.Length > 100)
        {
            throw new ValidationException("PRD-002",
                "El nombre del producto no puede superar los 100 caracteres.");
        }

        if (descripcion != null && descripcion.Length > 500)
        {
            throw new ValidationException("PRD-002",
                "La descripcion del producto no puede superar los 500 caracteres.");
        }

        if (precio <= 0)
        {
            throw new ValidationException("PRD-002", "El precio del producto debe ser mayor a cero.");
        }

        if (stock < 0)
        {
            throw new ValidationException("PRD-002", "El stock del producto no puede ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            throw new ValidationException("PRD-002", "La categoria del producto es obligatoria.");
        }
    }
}
