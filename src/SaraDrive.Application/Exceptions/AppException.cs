namespace SaraDrive.Application.Exceptions;

// Base for errors deliberately thrown from the Service layer. The global exception handler
// catches these and formats { message } with the right status code (no internal detail leaked).
public class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException() : AppException("Data tidak ditemukan", 404);
public class ConflictException(string message) : AppException(message, 409);
public class UnauthorizedException(string message) : AppException(message, 401);
public class BadRequestException(string message) : AppException(message, 400);
