using Notifications.API.Data;
using Notifications.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// REGISTRA EL INICIALIZADOR DE LA BASE.
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddScoped<NotificationRepository>();

// CONFIGURA EL CLIENTE PARA COMUNICARSE CON USERS.API.
var usersApiBaseUrl = builder.Configuration["UsersApi:BaseUrl"];

builder.Services.AddHttpClient<UserApiClient>(client =>
{
    client.BaseAddress = new Uri(
        usersApiBaseUrl ?? "http://localhost:5290");
});

var app = builder.Build();

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