using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Products.API.ExceptionHandlers;
using Products.API.Middleware;
using Serilog;
using Serilog.Events;

namespace Products.API.Extensions;

public static class MiddlewareExtensions
{
    public static void UseAppMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (context, _, exception) =>
            {
                if (exception != null || context.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }
                if (context.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }
                return LogEventLevel.Information;
            };
        });
        app.UseExceptionHandler();
        app.UseStatusCodePages(async statusContext =>
        {
            var context = statusContext.HttpContext;
            var problema = RespuestaError.CrearErrorHttp(context, context.Response.StatusCode);
            await RespuestaError.EscribirAsync(context, context.Response.StatusCode,
                problema.ErrorCode, problema.ErrorMessage, context.RequestAborted);
        });
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registro => registro.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registro => registro.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
    }
}
