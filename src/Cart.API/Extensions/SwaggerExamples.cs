using System.Text.Json;
using Cart.API.DTOs;
using Cart.API.ExceptionHandlers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cart.API.Extensions;

public class SwaggerExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        const string usuarioId = "a1b2c3d4-0000-0000-0000-111122223333";
        const string productoId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        operation.Responses ??= new OpenApiResponses();
        string accion = context.MethodInfo.Name;
        string ruta = ("/" + context.ApiDescription.RelativePath)
            .Replace("{userId}", usuarioId).Replace("{productId}", productoId);
        var carrito = new
        {
            usuarioId,
            items = new[] { new { productoId, cantidad = accion == "Actualizar" ? 4 : 2 } },
            fechaActualizacion = "2026-11-06T12:00:00Z"
        };
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
            object ejemplo = carrito;
            string tipo = "application/json";
            if (estado >= 400)
            {
                string codigo = "CRT-004";
                string mensaje = "Cantidad o datos del carrito invalidos.";
                if (estado == 404)
                {
                    codigo = accion == "Agregar" ? "CRT-002" : "CRT-001";
                    mensaje = accion == "Agregar" ? "Producto no encontrado." : "Carrito no encontrado.";
                }
                if (estado == 422) { codigo = "CRT-003"; mensaje = "Stock insuficiente para agregar al carrito."; }
                if (estado == 500) { codigo = "CRT-005"; mensaje = "Error interno al procesar el carrito."; }
                ejemplo = CrearError(ruta, estado, codigo, mensaje);
                esquema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository);
                tipo = "application/problem+json";
                respuesta.Description = codigo + ": " + mensaje;
            }
            respuesta.Content.Clear();
            var media = new OpenApiMediaType
            {
                Schema = esquema,
                Example = JsonSerializer.SerializeToNode(ejemplo, JsonSerializerOptions.Web)
            };
            if (estado == 404 && accion == "Actualizar")
            {
                respuesta.Description = "CRT-001: Carrito no encontrado. CRT-002: Producto no encontrado.";
                media.Example = null;
                media.Examples = new Dictionary<string, IOpenApiExample>
                {
                    ["carritoNoEncontrado"] = new OpenApiExample
                    {
                        Summary = "CRT-001",
                        Value = JsonSerializer.SerializeToNode(CrearError(ruta, 404, "CRT-001", "Carrito no encontrado."), JsonSerializerOptions.Web)
                    },
                    ["productoNoEncontrado"] = new OpenApiExample
                    {
                        Summary = "CRT-002",
                        Value = JsonSerializer.SerializeToNode(CrearError(ruta, 404, "CRT-002", "Producto no encontrado."), JsonSerializerOptions.Web)
                    }
                };
            }
            respuesta.Content[tipo] = media;
        }
        if (operation.RequestBody != null && operation.RequestBody.Content != null)
        {
            object datos = accion == "Agregar" ? new { productoId, cantidad = 2 } : new { cantidad = 4 };
            foreach (var contenido in operation.RequestBody.Content.Values)
            {
                contenido.Example = JsonSerializer.SerializeToNode(datos, JsonSerializerOptions.Web);
            }
        }
        if (operation.Parameters != null)
        {
            foreach (var parametro in operation.Parameters)
            {
                if (parametro is OpenApiParameter valor && (valor.Name == "userId" || valor.Name == "productId"))
                {
                    valor.Example = JsonSerializer.SerializeToNode(valor.Name == "userId" ? usuarioId : productoId);
                }
            }
        }
    }

    private static ErrorResponse CrearError(string ruta, int estado, string codigo, string mensaje)
    {
        var http = new DefaultHttpContext();
        http.Request.Path = ruta;
        http.TraceIdentifier = "ejemplo-correlacion";
        return RespuestaError.CrearProblema(http, estado, codigo, mensaje);
    }
}
