using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Cart.API.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Cart.API")
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] Service={Service} Endpoint={Endpoint} CorrelationId={CorrelationId} errorCode={errorCode} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(new JsonFormatter(), "logs/cart-.json", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        builder.Host.UseSerilog();
    }
}
