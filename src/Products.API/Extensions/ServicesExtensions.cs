using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Products.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<ProductRepository>();
        services.AddScoped<ProductoService>();

        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problema = RespuestaError.CrearProblema(context.HttpContext,
                    400, "PRD-002", "Los datos del producto no son validos.");
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
        services.AddSwaggerGen();

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database", "ready"],
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck<ApiStatusCheck>("api-status", tags: ["api", "live"]);

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(600);
            setup.AddHealthCheckEndpoint("Products.API", "/health");
        }).AddInMemoryStorage();
    }
}
