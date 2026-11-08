using Notifications.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// REGISTRA EL INICIALIZADOR DE LA BASE.
builder.Services.AddSingleton<DatabaseInitializer>();

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