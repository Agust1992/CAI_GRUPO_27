using Users.API.Data;
using Microsoft.AspNetCore.Identity;
using Users.API.Models;
using Users.API.Services;
using Users.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
var builder = WebApplication.CreateBuilder(args);

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
