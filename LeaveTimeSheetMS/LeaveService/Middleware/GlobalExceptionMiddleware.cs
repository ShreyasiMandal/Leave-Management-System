using System.Net;
using System.Text.Json;

namespace LeaveService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            ctx.Response.ContentType = "application/json";
            var (code, msg) = ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Unexpected error.")
            };
            ctx.Response.StatusCode = (int)code;
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                StatusCode = (int)code,
                Message = msg,
                Timestamp = DateTime.UtcNow
            }));
        }
    }
}