namespace ApiGateway.Middleware;

/// <summary>
/// Logs every request passing through the gateway.
/// Useful for debugging which service is being called.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        var start = DateTime.UtcNow;

        _logger.LogInformation(
            "Gateway → {Method} {Path} | Started: {Time}",
            method, path, start.ToString("HH:mm:ss"));

        await _next(context);

        var duration = (DateTime.UtcNow - start).TotalMilliseconds;
        var status = context.Response.StatusCode;

        _logger.LogInformation(
            "Gateway ← {Method} {Path} | Status: {Status} | Duration: {Duration}ms",
            method, path, status, duration);
    }
}