using Users.API.Data;
using Microsoft.AspNetCore.Identity;
using Users.API.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// REGISTRA EL INICIALIZADOR DE LA BASE.
builder.Services.AddSingleton<DatabaseInitializer>();

// REGISTRA EL REPOSITORIO DE USUARIOS.
builder.Services.AddScoped<UserRepository>();

// REGISTRA EL COMPONENTE PARA CREAR Y VERIFICAR HASHES.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

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
