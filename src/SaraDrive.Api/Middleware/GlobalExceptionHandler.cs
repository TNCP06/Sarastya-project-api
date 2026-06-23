using Microsoft.AspNetCore.Diagnostics;
using SaraDrive.Application.Exceptions;

namespace SaraDrive.Api.Middleware;

// AppException (400/401/404/409) → { message } with that status, no internal detail.
// Anything else → log full detail to Serilog, return a generic 500.
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception exception, CancellationToken ct)
    {
        if (exception is AppException appEx)
        {
            ctx.Response.StatusCode = appEx.StatusCode;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { message = appEx.Message }, ct);
            return true;
        }

        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new { message = "Terjadi kesalahan pada server" }, ct);
        return true;
    }
}
