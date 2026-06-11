using System.Net;
using System.Text.Json;
using StudyPlanner.Core.Exceptions;
using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.API.Middleware;

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
        var traceId = context.TraceIdentifier;
        var (statusCode, message, errors) = MapException(exception);

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
            message = "An unexpected error occurred.";
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception. TraceId: {TraceId}", traceId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ErrorResponse.Create(statusCode, message, traceId, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string Message, IEnumerable<string>? Errors) MapException(Exception exception) =>
        exception switch
        {
            NotFoundException notFound => ((int)HttpStatusCode.NotFound, notFound.Message, null),
            BadRequestException badRequest => ((int)HttpStatusCode.BadRequest, badRequest.Message, null),
            UnauthorizedException unauthorized => ((int)HttpStatusCode.Unauthorized, unauthorized.Message, null),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };
}
