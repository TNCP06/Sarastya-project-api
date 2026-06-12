using Microsoft.AspNetCore.Diagnostics;
using Projektask.Application.Exceptions;

namespace Projektask.Api.Middleware;

// Menangkap semua exception yang tidak tertangani di controller/service
// AppException (404, 409, 401) → format sesuai kontrak bagian 4, tanpa detail internal
// Exception lain → log lengkap ke Serilog, response generik 500
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is AppException appEx)
        {
            ctx.Response.StatusCode = appEx.StatusCode;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { message = appEx.Message }, ct);
            return true;
        }

        // Error tak terduga — catat detail di Serilog, jangan bocorkan ke client
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(
            new { message = "Terjadi kesalahan pada server" }, ct);
        return true;
    }
}
