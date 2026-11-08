using Notifications.API.Data;
using Notifications.API.Services;
using Notifications.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Serilog;
using Serilog.Formatting.Compact;
using Notifications.API.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURA SERILOG PARA CONSOLA Y ARCHIVO JSON.
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .Enrich.WithProperty("Service", "Notifications.API")
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            new CompactJsonFormatter(),
            "logs/notifications-.json",
            rollingInterval: RollingInterval.Day);
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var ruta = context.Description.RelativePath?.TrimStart('/');
        var metodo = context.Description.HttpMethod;

        void AgregarEjemplo(string codigoEstado, string json)
        {
            if (!operation.Responses.TryGetValue(codigoEstado, out var respuesta)
                || respuesta.Content is null)
            {
                return;
            }

            foreach (var contenido in respuesta.Content.Values)
            {
                contenido.Example = JsonNode.Parse(json);
            }
        }

        if (metodo == "POST" && ruta == "api/notifications/send")
        {
            AgregarEjemplo("400", """
                {
                  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                  "title": "Bad Request",
                  "status": 400,
                  "detail": "Los datos de la notificación son inválidos.",
                  "instance": "/api/notifications/send",
                  "errorCode": "NTF-002",
                  "errorMessage": "Los datos de la notificación son inválidos.",
                  "correlationId": "7f46ea18-8a4c-42db-b43b-731f8b871234"
                }
                """);

            AgregarEjemplo("404", """
                {
                  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                  "title": "Not Found",
                  "status": 404,
                  "detail": "El recurso solicitado no fue encontrado.",
                  "instance": "/api/notifications/send",
                  "errorCode": "NTF-001",
                  "errorMessage": "Usuario no encontrado.",
                  "correlationId": "7f46ea18-8a4c-42db-b43b-731f8b871234"
                }
                """);

            AgregarEjemplo("500", """
                {
                  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                  "title": "Internal Server Error",
                  "status": 500,
                  "detail": "Error interno al procesar la notificación.",
                  "instance": "/api/notifications/send",
                  "errorCode": "NTF-004",
                  "errorMessage": "Error interno al procesar la notificación.",
                  "correlationId": "7f46ea18-8a4c-42db-b43b-731f8b871234"
                }
                """);
        }

        if (metodo == "GET" && ruta == "api/notifications/{userId}")
        {
            AgregarEjemplo("404", """
                {
                  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                  "title": "Not Found",
                  "status": 404,
                  "detail": "El recurso solicitado no fue encontrado.",
                  "instance": "/api/notifications/e7ed291e-841e-4592-ab9e-4b1614550aea",
                  "errorCode": "NTF-003",
                  "errorMessage": "No se encontraron notificaciones para el usuario.",
                  "correlationId": "7f46ea18-8a4c-42db-b43b-731f8b871234"
                }
                """);

            AgregarEjemplo("500", """
                {
                  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                  "title": "Internal Server Error",
                  "status": 500,
                  "detail": "Error interno al procesar la notificación.",
                  "instance": "/api/notifications/e7ed291e-841e-4592-ab9e-4b1614550aea",
                  "errorCode": "NTF-004",
                  "errorMessage": "Error interno al procesar la notificación.",
                  "correlationId": "7f46ea18-8a4c-42db-b43b-731f8b871234"
                }
                """);
        }

        return Task.CompletedTask;
    });
});

// REGISTRA EL INICIALIZADOR DE LA BASE.
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddScoped<NotificationRepository>();

builder.Services.AddScoped<NotificationService>();

// REGISTRA LA COMPROBACIÓN DE SALUD DE LA BASE DE DATOS.
builder.Services.AddHealthChecks()
    .AddCheck(
        "api",
        () => HealthCheckResult.Healthy(),
        tags: new[] { "live" })
    .AddCheck<NotificationsDatabaseHealthCheck>(
        "base de notificaciones",
        tags: new[] { "ready" });

// CONFIGURA EL CLIENTE PARA COMUNICARSE CON USERS.API.
var usersApiBaseUrl = builder.Configuration["UsersApi:BaseUrl"];

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<UserApiClient>(client =>
{
    client.BaseAddress = new Uri(
        usersApiBaseUrl ?? "http://localhost:5290");
});

// PERSONALIZA LA RESPUESTA CUANDO FALLAN LAS VALIDACIONES.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = "Los datos de la notificación son inválidos.";

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = message,
            Instance = context.HttpContext.Request.Path,
            ErrorCode = "NTF-002",
            ErrorMessage = message,
            CorrelationId = context.HttpContext.TraceIdentifier
        };

        var result = new ObjectResult(errorResponse)
        {
            StatusCode = 400
        };

        result.ContentTypes.Add("application/problem+json");

        return result;
    };
});

// REGISTRA LOS MANEJADORES DE ERRORES.
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// REGISTRA CADA PEDIDO HTTP CON SERILOG.
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Endpoint", httpContext.Request.Path.Value);
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
    };
});

app.UseExceptionHandler();

// AGREGA UN IDENTIFICADOR A CADA PEDIDO.
app.Use(async (context, next) =>
{
    var correlationId =
        context.Request.Headers["X-Correlation-Id"].FirstOrDefault()?.Trim();

    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
    }

    context.TraceIdentifier = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;

    await next();
});

// CREA LA TABLA DE NOTIFICACIONES AL INICIAR LA API.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>()
        .Initialize();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// PREPARA LA RESPUESTA DE SALUD EN FORMATO JSON.
Task EscribirEstadoSalud(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var respuesta = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString()
    });

    return context.Response.WriteAsync(respuesta);
}


// PUBLICA EL ESTADO GENERAL DE LA API Y LA BASE DE DATOS.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = EscribirEstadoSalud
});
// PUBLICA LA COMPROBACIÓN DE LA BASE DE DATOS.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = EscribirEstadoSalud
});

// PUBLICA LA COMPROBACIÓN DE QUE LA API ESTÁ ENCENDIDA.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = EscribirEstadoSalud
});

app.Run();