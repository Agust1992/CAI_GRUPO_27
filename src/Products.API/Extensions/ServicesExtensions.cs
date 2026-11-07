using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Products.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<ProductRepository>();
        services.AddScoped<ProductoService>();
        services.AddHttpContextAccessor();
        services.AddHttpClient<OrdersClient>((provider, client) =>
        {
            string? direccion = provider.GetRequiredService<IConfiguration>()["Services:Orders"];
            if (string.IsNullOrWhiteSpace(direccion))
            {
                throw new InvalidOperationException("Falta configurar Services:Orders.");
            }
            client.BaseAddress = new Uri(direccion);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ProductoService>>();
                logger.LogWarning("{errorCode}: Datos del producto invalidos", "PRD-002");
                var problema = RespuestaError.CrearProblema(context.HttpContext,
                    400, "PRD-002", "Los datos del producto no son validos.");
                var respuesta = new ObjectResult(problema) { StatusCode = 400 };
                respuesta.ContentTypes.Add("application/problem+json");
                return respuesta;
            };
        });
        services.AddTransient<IClientErrorFactory, ClientErrorFactory>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Products.API.xml"));
            options.OperationFilter<SwaggerExamples>();
        });

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database", "ready"],
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck<ApiStatusCheck>("api-status", tags: ["api", "live"]);
    }
}
