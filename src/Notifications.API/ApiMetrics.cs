using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Notifications.API;

/// <summary>
/// Métricas básicas de tráfico y duración para Notifications.API.
/// </summary>
internal static class ApiMetrics
{
    private static readonly Meter Meter = new("Notifications.API");
    private static readonly Counter<long> Requests =
        Meter.CreateCounter<long>("notifications.http.requests");
    private static readonly Counter<long> Errors =
        Meter.CreateCounter<long>("notifications.http.errors");
    private static readonly Histogram<double> Duration =
        Meter.CreateHistogram<double>("notifications.http.request.duration", "ms");

    /// <summary>
    /// Registra el resultado y la duración de una solicitud.
    /// </summary>
    internal static void Record(HttpContext context, double durationMilliseconds)
    {
        var endpoint = context.GetEndpoint() is RouteEndpoint route
            ? route.RoutePattern.RawText ?? "unmatched"
            : "unmatched";
        var tags = new TagList
        {
            { "service", "Notifications.API" },
            { "http.request.method", context.Request.Method },
            { "http.route", endpoint },
            { "http.response.status_code", context.Response.StatusCode }
        };

        if (context.Items.TryGetValue("ErrorCode", out var errorCode))
        {
            tags.Add("error.code", errorCode?.ToString() ?? "unknown");
        }

        Requests.Add(1, tags);
        Duration.Record(durationMilliseconds, tags);

        if (context.Response.StatusCode >= 400)
        {
            Errors.Add(1, tags);
        }
    }
}
