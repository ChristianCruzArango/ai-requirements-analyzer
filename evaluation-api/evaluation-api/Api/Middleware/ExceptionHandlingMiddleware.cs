using System.Net;
using System.Text.Json;
using evaluation_api.Api.Common;
using evaluation_api.Domain.Exceptions;

namespace evaluation_api.Api.Middleware;

/// <summary>
/// Middleware para manejo centralizado de excepciones
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "❌ Excepción capturada: {Message}", exception.Message);

        var response = exception switch
        {
            ValidationException validationEx => ApiResponse<object>.ErrorResponse(
                validationEx.Message,
                validationEx.ErrorCode,
                validationEx.StatusCode,
                validationEx.Errors
            ),

            NotFoundException notFoundEx => ApiResponse<object>.ErrorResponse(
                notFoundEx.Message,
                notFoundEx.ErrorCode,
                notFoundEx.StatusCode
            ),

            AIServiceException aiEx => ApiResponse<object>.ErrorResponse(
                aiEx.Message,
                aiEx.ErrorCode,
                aiEx.StatusCode
            ),

            DatabaseException dbEx => ApiResponse<object>.ErrorResponse(
                dbEx.Message,
                dbEx.ErrorCode,
                dbEx.StatusCode
            ),

            BaseException baseEx => ApiResponse<object>.ErrorResponse(
                baseEx.Message,
                baseEx.ErrorCode,
                baseEx.StatusCode
            ),

            _ => ApiResponse<object>.ErrorResponse(
                "Error interno del servidor",
                "INTERNAL_SERVER_ERROR",
                500
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Error?.StatusCode ?? 500;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

/// <summary>
/// Extension method para registrar el middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
