namespace Projektask.Application.Exceptions;

// Base class untuk semua error yang "sengaja" dilempar dari Service layer
// GlobalExceptionHandler akan tangkap ini dan format response sesuai kontrak
public class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

// 404 — data tidak ada / bukan milik user ini
public class NotFoundException() : AppException("Data tidak ditemukan", 404);

// 409 — email sudah terdaftar
public class ConflictException(string message) : AppException(message, 409);

// 401 — email/password salah
public class UnauthorizedException(string message) : AppException(message, 401);
