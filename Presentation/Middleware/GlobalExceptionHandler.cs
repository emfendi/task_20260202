using System.ComponentModel.DataAnnotations;
using EmployeeContactApi.Domain.Exceptions;
using EmployeeContactApi.Presentation.Dto;
using Microsoft.AspNetCore.Diagnostics;

namespace EmployeeContactApi.Presentation.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, error, message) = exception switch
        {
            DuplicateEmailException ex => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            InvalidDataFormatException ex => (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, "Validation Error", ex.Message),
            ArgumentException ex => (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unexpected error occurred");
        }
        else
        {
            _logger.LogWarning("Request error: {Message}", message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(
            Status: statusCode,
            Error: error,
            Message: message,
            Path: httpContext.Request.Path
        ), cancellationToken);

        return true;
    }
}
