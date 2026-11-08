using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text.Json.Nodes;
using Users.API.Data;
using Users.API.DTOs;
using Users.API.ExceptionHandlers;
using Users.API.Models;
using Users.API.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Users.API.HealthChecks;
using Microsoft.OpenApi;
var builder = WebApplication.CreateBuilder(args);

// CONFIGURA SERILOG PARA CONSOLA Y ARCHIVO JSON.
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .Enrich.WithProperty("Service", "Users.API")
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            new CompactJsonFormatter(),
            "logs/users-.json",
            rollingInterval: RollingInterval.Day);
});

// Add services to the container.

builder.Services.AddControllers();

// PERSONALIZA LA RESPUESTA CUANDO LOS DATOS NO PASAN LA VALIDACIÓN.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "Los datos del usuario son inválidos.",
            Instance = context.HttpContext.Request.Path.Value ?? string.Empty,
            ErrorCode = "USR-002",
            ErrorMessage = "Los datos del usuario son inválidos.",
            CorrelationId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var ruta = context.Description.RelativePath?.Trim('/');
        var metodo = context.Description.HttpMethod;

        // EJEMPLOS PARA REGISTRAR UN USUARIO.
        if (metodo == "POST" && ruta == "api/users/register")
        {
            if (operation.Responses.TryGetValue("201", out var respuestaRegistro) &&
                respuestaRegistro.Content.TryGetValue(
                    "application/json",
                    out var contenidoRegistro))
            {
                contenidoRegistro.Example = new JsonObject
                {
                    ["id"] = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                    ["nombre"] = "Ana",
                    ["apellido"] = "Pérez",
                    ["email"] = "ana.perez@example.com",
                    ["fechaRegistro"] = "2026-11-08T18:00:00Z",
                    ["activo"] = true
                };
            }

            AgregarEjemploError(
                operation, "400", "Bad Request", 400,
                "Los datos del usuario son inválidos.",
                "/api/users/register", "USR-002",
                "Los datos del usuario son inválidos.");

            AgregarEjemploError(
                operation, "409", "Conflict", 409,
                "Ya existe un recurso con esos datos.",
                "/api/users/register", "USR-001",
                "El email ya está registrado.");

            AgregarEjemploError(
                operation, "500", "Internal Server Error", 500,
                "Ocurrió un error inesperado.",
                "/api/users/register", "USR-006",
                "Error interno al procesar el usuario.");
        }

        // EJEMPLOS PARA INICIAR SESIÓN.
        if (metodo == "POST" && ruta == "api/users/login")
        {
            if (operation.Responses.TryGetValue("200", out var respuestaLogin) &&
                respuestaLogin.Content.TryGetValue(
                    "application/json",
                    out var contenidoLogin))
            {
                contenidoLogin.Example = new JsonObject
                {
                    ["id"] = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                    ["nombre"] = "Ana",
                    ["apellido"] = "Pérez",
                    ["email"] = "ana.perez@example.com"
                };
            }

            AgregarEjemploError(
                operation, "400", "Bad Request", 400,
                "Los datos del usuario son inválidos.",
                "/api/users/login", "USR-002",
                "Los datos del usuario son inválidos.");

            AgregarEjemploError(
                operation, "401", "Unauthorized", 401,
                "Las credenciales no son válidas.",
                "/api/users/login", "USR-003",
                "Credenciales incorrectas.");

            AgregarEjemploError(
                operation, "403", "Forbidden", 403,
                "El acceso está prohibido.",
                "/api/users/login", "USR-004",
                "Usuario bloqueado por demasiados intentos fallidos.");

            AgregarEjemploError(
                operation, "500", "Internal Server Error", 500,
                "Ocurrió un error inesperado.",
                "/api/users/login", "USR-006",
                "Error interno al procesar el usuario.");

            AgregarEjemplosBloqueo(operation);
        }

        // EJEMPLO DE RESPUESTA AL BUSCAR UN USUARIO POR ID.
        if (metodo == "GET" && ruta == "api/users/{id}")
        {
            if (operation.Responses.TryGetValue("200", out var respuestaGet) &&
                respuestaGet.Content.TryGetValue(
                    "application/json",
                    out var contenidoGet))
            {
                contenidoGet.Example = new JsonObject
                {
                    ["id"] = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                    ["nombre"] = "Ana",
                    ["apellido"] = "Pérez",
                    ["email"] = "ana.perez@example.com"
                };
            }

            AgregarEjemploError(
                operation, "500", "Internal Server Error", 500,
                "Ocurrió un error inesperado.",
                "/api/users/{id}", "USR-006",
                "Error interno al procesar el usuario.");

            if (operation.Responses.TryGetValue("404", out var respuesta404) &&
                respuesta404.Content.TryGetValue(
                    "application/problem+json",
                    out var contenido404))
            {
                contenido404.Example = new JsonObject
                {
                    ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    ["title"] = "Not Found",
                    ["status"] = 404,
                    ["traceId"] = "00-00000000000000000000000000000000-0000000000000000-00"
                };
            }
        }

        return Task.CompletedTask;
    });
});

// REGISTRA EL SERVICIO DE USUARIOS.
builder.Services.AddScoped<UserService>();

// REGISTRA EL INICIALIZADOR DE LA BASE.
builder.Services.AddSingleton<DatabaseInitializer>();

// REGISTRA EL REPOSITORIO DE USUARIOS.
builder.Services.AddScoped<UserRepository>();

// REGISTRA EL COMPONENTE PARA CREAR Y VERIFICAR HASHES.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// REGISTRA EL MANEJADOR DE ERRORES DE NEGOCIO.
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// REGISTRA LAS COMPROBACIONES DE SALUD.
builder.Services.AddHealthChecks()
    .AddCheck(
        "api",
        () => HealthCheckResult.Healthy(),
        tags: new[] { "live" })
    .AddCheck<UsersDatabaseHealthCheck>(
        "base de usuarios",
        tags: new[] { "ready" });

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

// ACTIVA EL MANEJO GLOBAL DE EXCEPCIONES.
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

// CREA LA TABLA DE USUARIOS AL INICIAR LA API.
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

    // MUESTRA LA DOCUMENTACIÓN INTERACTIVA DE LA API.
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// PUBLICA EL ESTADO GENERAL DE LA API Y LA BASE.
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

static Task EscribirEstadoSalud(
    HttpContext context,
    HealthReport report)
{
    context.Response.ContentType = "application/json";

    var respuesta = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString()
    });

    return context.Response.WriteAsync(respuesta);
}

static void AgregarEjemploError(
    OpenApiOperation operation,
    string codigoRespuesta,
    string titulo,
    int estado,
    string detalle,
    string ruta,
    string codigoError,
    string mensajeError)
{
    if (!operation.Responses.TryGetValue(codigoRespuesta, out var respuesta))
    {
        return;
    }

    if (!respuesta.Content.TryGetValue(
            "application/problem+json",
            out var contenido))
    {
        return;
    }

    contenido.Example = new JsonObject
    {
        ["type"] = estado switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "https://tools.ietf.org/html/rfc7231"
        },
        ["title"] = titulo,
        ["status"] = estado,
        ["detail"] = detalle,
        ["instance"] = ruta,
        ["errorCode"] = codigoError,
        ["errorMessage"] = mensajeError,
        ["correlationId"] = "11111111-1111-1111-1111-111111111111"
    };
}

static void AgregarEjemplosBloqueo(OpenApiOperation operation)
{
    if (!operation.Responses.TryGetValue("403", out var respuesta))
    {
        return;
    }

    if (!respuesta.Content.TryGetValue(
            "application/problem+json",
            out var contenido))
    {
        return;
    }

    contenido.Example = null;
    contenido.Examples = new Dictionary<string, IOpenApiExample>
    {
        ["USR-004 - Intentos fallidos"] = new OpenApiExample
        {
            Summary = "Usuario bloqueado por intentos fallidos",
            Value = new JsonObject
            {
                ["type"] = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                ["title"] = "Forbidden",
                ["status"] = 403,
                ["detail"] = "El acceso está prohibido.",
                ["instance"] = "/api/users/login",
                ["errorCode"] = "USR-004",
                ["errorMessage"] = "Usuario bloqueado por demasiados intentos fallidos.",
                ["correlationId"] = "11111111-1111-1111-1111-111111111111"
            }
        },
        ["USR-005 - Detección de fraude"] = new OpenApiExample
        {
            Summary = "Usuario bloqueado por detección de fraude",
            Value = new JsonObject
            {
                ["type"] = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                ["title"] = "Forbidden",
                ["status"] = 403,
                ["detail"] = "El acceso está prohibido.",
                ["instance"] = "/api/users/login",
                ["errorCode"] = "USR-005",
                ["errorMessage"] = "Usuario bloqueado por detección de fraude.",
                ["correlationId"] = "11111111-1111-1111-1111-111111111111"
            }
        }
    };
}