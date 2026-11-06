using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<CarritoRepository>();
        services.AddScoped<CarritoService>();
        services.AddHttpContextAccessor();
        services.AddHttpClient<ProductosClient>((provider, client) =>
        {
            string? direccion = provider.GetRequiredService<IConfiguration>()["Services:Products"];
            if (string.IsNullOrWhiteSpace(direccion))
            {
                throw new InvalidOperationException("Falta configurar Services:Products.");
            }
            client.BaseAddress = new Uri(direccion);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CarritoService>>();
                logger.LogWarning("{errorCode}: Datos del carrito invalidos", "CRT-004");
                var problema = RespuestaError.CrearProblema(context.HttpContext,
                    400, "CRT-004", "Cantidad o datos del carrito invalidos.");
                var respuesta = new ObjectResult(problema) { StatusCode = 400 };
                respuesta.ContentTypes.Add("application/problem+json");
                return respuesta;
            };
        });
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Cart.API.xml")));
        services.AddHealthChecks()
            .AddCheck<ApiStatusCheck>("api-status", tags: ["live"])
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["ready"], timeout: TimeSpan.FromSeconds(5));
    }
}
