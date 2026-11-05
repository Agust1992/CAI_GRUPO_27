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
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("El nombre del producto es obligatorio.");
            }

            if (request.Price < 0 || request.Stock < 0)
            {
                return Results.BadRequest("El precio y el stock no pueden ser negativos.");
            }

            var product = await repository.CreateAsync(request);
            return Results.Created($"/products/{product.Id}", product);
        });

        group.MapPut("/{id:int}", async (int id, UpdateProductRequest request, ProductRepository repository) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("El nombre del producto es obligatorio.");
            }

            if (request.Price < 0 || request.Stock < 0)
            {
                return Results.BadRequest("El precio y el stock no pueden ser negativos.");
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
}
