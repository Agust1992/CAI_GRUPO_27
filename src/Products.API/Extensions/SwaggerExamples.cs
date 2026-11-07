using System.Text.Json;
using Microsoft.OpenApi;
using Products.API.DTOs;
using Products.API.ExceptionHandlers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Products.API.Extensions;

public class SwaggerExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        const string id = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        string accion = context.MethodInfo.Name;
        operation.Responses ??= new OpenApiResponses();
        string ruta = "/" + context.ApiDescription.RelativePath;
        ruta = ruta.Replace("{id}", id);
        var producto = new
        {
            id,
            nombre = "Notebook",
            descripcion = "Producto de ejemplo",
            precio = 1500.50m,
            stock = 10,
            categoria = "Tecnologia",
            fechaCreacion = "2026-11-06T12:00:00Z"
        };
        var datos = new { producto.nombre, producto.descripcion, producto.precio, producto.stock, producto.categoria };

        if (context.ApiDescription.HttpMethod == "POST" || context.ApiDescription.HttpMethod == "PUT")
        {
            operation.Responses["415"] = new OpenApiResponse { Description = "Formato de solicitud no soportado." };
        }
        foreach (var entrada in operation.Responses)
        {
            if (!int.TryParse(entrada.Key, out int estado) || entrada.Value is not OpenApiResponse respuesta)
            {
                continue;
            }
            respuesta.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (estado == 204)
            {
                respuesta.Content.Clear();
                continue;
            }

            IOpenApiSchema? esquema = null;
            foreach (var contenido in respuesta.Content.Values)
            {
                esquema = contenido.Schema;
                break;
            }
            object ejemplo = producto;
            string tipo = "application/json";
            if (estado >= 400)
            {
                string codigo = "PRD-002";
                string mensaje = "Los datos del producto son invalidos.";
                if (estado == 404) { codigo = "PRD-001"; mensaje = "Producto no encontrado."; }
                if (estado == 409 && accion == "Eliminar")
                {
                    codigo = "PRD-004";
                    mensaje = "El producto tiene ordenes activas y no puede eliminarse.";
                }
                else if (estado == 409)
                {
                    codigo = "PRD-003";
                    mensaje = "Ya existe un producto con ese nombre en la categoria.";
                }
                if (estado == 500) { codigo = "PRD-005"; mensaje = "Error interno al procesar el producto."; }
                var http = new DefaultHttpContext();
                http.Request.Path = ruta;
                http.TraceIdentifier = "ejemplo-correlacion";
                ejemplo = RespuestaError.CrearProblema(http, estado, codigo, mensaje);
                esquema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository);
                tipo = "application/problem+json";
                respuesta.Description = codigo + ": " + mensaje;
            }
            else if (accion == "Listar")
            {
                ejemplo = new[] { producto };
            }
            respuesta.Content.Clear();
            respuesta.Content[tipo] = new OpenApiMediaType
            {
                Schema = esquema,
                Example = JsonSerializer.SerializeToNode(ejemplo, JsonSerializerOptions.Web)
            };
        }
        if (operation.RequestBody != null && operation.RequestBody.Content != null)
        {
            foreach (var contenido in operation.RequestBody.Content.Values)
            {
                contenido.Example = JsonSerializer.SerializeToNode(datos, JsonSerializerOptions.Web);
            }
        }
        if (operation.Parameters != null)
        {
            foreach (var parametro in operation.Parameters)
            {
                if (parametro is OpenApiParameter valor && valor.Name == "id")
                {
                    valor.Example = JsonSerializer.SerializeToNode(id);
                }
            }
        }
    }
}
