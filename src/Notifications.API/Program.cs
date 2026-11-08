using Notifications.API.Data;
using Notifications.API.Services;
using Notifications.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// REGISTRA EL INICIALIZADOR DE LA BASE.
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddScoped<NotificationRepository>();

builder.Services.AddScoped<NotificationService>();

// CONFIGURA EL CLIENTE PARA COMUNICARSE CON USERS.API.
var usersApiBaseUrl = builder.Configuration["UsersApi:BaseUrl"];

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

var app = builder.Build();

app.UseExceptionHandler();

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
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();