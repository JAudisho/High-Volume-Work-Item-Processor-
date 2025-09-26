using System.Net;
using System.Text.Json;

namespace AtlasQueue.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (Exception ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new { error = ex.Message });
            await ctx.Response.WriteAsync(payload);
        }
    }
}
