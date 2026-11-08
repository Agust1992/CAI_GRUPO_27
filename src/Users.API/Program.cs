using Users.API.Data;
using Microsoft.AspNetCore.Identity;
using Users.API.Models;
using Users.API.Services;
using Users.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Serilog;
using Serilog.Formatting.Compact;
var builder = WebApplication.CreateBuilder(args);

// CONFIGURA SERILOG PARA CONSOLA Y ARCHIVO JSON.
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
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
builder.Services.AddOpenApi();

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

app.Run();
