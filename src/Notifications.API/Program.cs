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
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
        .Enrich.WithProperty("Service", "Notifications.API")
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] Service={Service} Endpoint={Endpoint} CorrelationId={CorrelationId} ErrorCode={ErrorCode} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            new CompactJsonFormatter(),
            "logs/notifications-.json",
            rollingInterval: RollingInterval.Day);
});

// Add services to the container.
builder.Services.AddControllers(options => options.Filters.Add(new Notifications.API.ExceptionHandlers.UnsupportedMediaTypeResultFilter()));

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

        context.HttpContext.Items["ErrorCode"] = "NTF-002";

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
    options.GetLevel = (context, _, exception) =>
    {
        if (exception != null || context.Response.StatusCode >= 500)
        {
            return Serilog.Events.LogEventLevel.Error;
        }
        if (context.Response.StatusCode >= 400)
        {
            return Serilog.Events.LogEventLevel.Warning;
        }
        return Serilog.Events.LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Endpoint", httpContext.Request.Path.Value);
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        if (httpContext.Items.TryGetValue("ErrorCode", out var errorCode))
        {
            diagnosticContext.Set("ErrorCode", errorCode);
        }
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

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    using (Serilog.Context.LogContext.PushProperty("Endpoint", context.Request.Path.Value ?? string.Empty))
    {
        await next();
    }
});

// REGISTRA EL INICIO CON LA CORRELACIÓN YA ASIGNADA A LA RESPUESTA.
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Notifications.API.RequestStart");

    logger.LogInformation(
        "Inicio de solicitud HTTP {RequestMethod} en {Endpoint}. CorrelationId: {CorrelationId}",
        context.Request.Method,
        context.Request.Path.Value,
        context.TraceIdentifier);

    await next();
});
// AGREGA EL FORMATO COMÚN A ERRORES HTTP SIN RESPUESTA.
app.UseStatusCodePages(async statusContext =>
{
    var context = statusContext.HttpContext;
    var status = context.Response.StatusCode;
    if (status is not (404 or 405 or 415))
    {
        return;
    }

    var (errorCode, errorMessage, detail, type, title) = status switch
    {
        404 => (
            "NTF-001",
            "Usuario o recurso no encontrado.",
            "El recurso solicitado no fue encontrado.",
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Not Found"),
        405 => (
            "NTF-002",
            "Método HTTP no permitido.",
            "El método no está permitido para esta ruta.",
            "https://tools.ietf.org/html/rfc7231#section-6.5.5",
            "Method Not Allowed"),
        _ => (
            "NTF-002",
            "Tipo de contenido no soportado.",
            "Enviá los datos con Content-Type application/json.",
            "https://tools.ietf.org/html/rfc7231#section-6.5.13",
            "Unsupported Media Type")
    };

    context.Items["ErrorCode"] = errorCode;
    var response = new ErrorResponse
    {
        Type = type,
        Title = title,
        Status = status,
        Detail = detail,
        Instance = context.Request.Path.Value ?? string.Empty,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        CorrelationId = context.TraceIdentifier
    };

    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Notifications.API.HttpErrors");
    logger.LogWarning(
        "HTTP {StatusCode} en {Endpoint}. Código {ErrorCode}: {Message}. CorrelationId: {CorrelationId}",
        status,
        context.Request.Path,
        errorCode,
        errorMessage,
        context.TraceIdentifier);

    await context.Response.WriteAsJsonAsync(
        response,
        options: null,
        contentType: "application/problem+json");
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