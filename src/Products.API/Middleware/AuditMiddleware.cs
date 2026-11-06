using System.Text;
using System.Text.Json;

namespace Products.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    private static readonly HashSet<string> AuditMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE" };

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!AuditMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();
        var requestBody = await ReadBodyAsync(context.Request.Body);
        context.Request.Body.Position = 0;

        var originalResponseBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalResponseBody);

            _logger.LogInformation(
                "AUDIT {@Method} {@Path} {@StatusCode} {@RequestBody} {@ResponseBody}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                TryParseJson(requestBody),
                TryParseJson(responseBody));
        }
        finally
        {
            // El handler debe poder escribir la respuesta si una operacion falla.
            context.Response.Body = originalResponseBody;
        }
    }

    private static async Task<string> ReadBodyAsync(Stream body)
    {
        using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static object? TryParseJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<object>(raw);
        }
        catch
        {
            return raw;
        }
    }
}
