using Products.API.Data;
using Products.API.DTOs;

namespace Products.API.Extensions.Endpoints;

public static class ProductsEndpointsExtension
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (ProductRepository repository) =>
        {
            var products = await repository.GetAllAsync();
            return Results.Ok(products);
        });

        group.MapGet("/{id:int}", async (int id, ProductRepository repository) =>
        {
            var product = await repository.GetByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        });

        group.MapPost("/", async (CreateProductRequest request, ProductRepository repository) =>
        {
            string? error = ValidarProducto(request.Name, request.Description,
                request.Price, request.Stock, request.Category);
            if (error != null)
            {
                return Results.BadRequest(error);
            }

            var product = await repository.CreateAsync(request);
            return Results.Created($"/products/{product.Id}", product);
        });

        group.MapPut("/{id:int}", async (int id, UpdateProductRequest request, ProductRepository repository) =>
        {
            string? error = ValidarProducto(request.Name, request.Description,
                request.Price, request.Stock, request.Category);
            if (error != null)
            {
                return Results.BadRequest(error);
            }

            var updated = await repository.UpdateAsync(id, request);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ProductRepository repository) =>
        {
            var deleted = await repository.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    private static string? ValidarProducto(string? nombre, string? descripcion,
        decimal precio, int stock, string? categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "El nombre del producto es obligatorio.";
        }

        if (nombre.Length > 100)
        {
            return "El nombre del producto no puede superar los 100 caracteres.";
        }

        if (descripcion != null && descripcion.Length > 500)
        {
            return "La descripcion del producto no puede superar los 500 caracteres.";
        }

        if (precio <= 0)
        {
            return "El precio del producto debe ser mayor a cero.";
        }

        if (stock < 0)
        {
            return "El stock del producto no puede ser negativo.";
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            return "La categoria del producto es obligatoria.";
        }

        return null;
    }
}
