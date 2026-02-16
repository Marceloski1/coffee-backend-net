using System.Net;
using System.Text.Json;

namespace Coffee.Api.Middleware;

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
        _logger.LogError(exception, "Ocurrió un error no controlado: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ArgumentException => CreateErrorResponse(context, HttpStatusCode.BadRequest, "Solicitud inválida", exception.Message),
            KeyNotFoundException => CreateErrorResponse(context, HttpStatusCode.NotFound, "Recurso no encontrado", exception.Message),
            UnauthorizedAccessException => CreateErrorResponse(context, HttpStatusCode.Unauthorized, "No autorizado", exception.Message),
            InvalidOperationException => CreateErrorResponse(context, HttpStatusCode.BadRequest, "Operación inválida", exception.Message),
            TimeoutException => CreateErrorResponse(context, HttpStatusCode.RequestTimeout, "Tiempo de espera agotado", exception.Message),
            _ => CreateErrorResponse(context, HttpStatusCode.InternalServerError, "Error interno del servidor", "Ha ocurrido un error inesperado. Por favor, inténtelo más tarde.")
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static object CreateErrorResponse(HttpContext context, HttpStatusCode statusCode, string title, string message)
    {
        context.Response.StatusCode = (int)statusCode;

        return new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title = title,
            status = (int)statusCode,
            detail = message,
            traceId = context.TraceIdentifier,
            instance = context.Request.Path
        };
    }
}