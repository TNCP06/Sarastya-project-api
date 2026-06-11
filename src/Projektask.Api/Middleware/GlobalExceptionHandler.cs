using Microsoft.AspNetCore.Diagnostics;

namespace Projektask.Api.Middleware;

// Menangkap semua exception yang tidak tertangani — format response sesuai kontrak bagian 4
// Detail error di-log ke Serilog, TIDAK pernah dikirim ke client
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";

        await ctx.Response.WriteAsJsonAsync(
            new { message = "Terjadi kesalahan pada server" }, ct);

        return true;
    }
}
