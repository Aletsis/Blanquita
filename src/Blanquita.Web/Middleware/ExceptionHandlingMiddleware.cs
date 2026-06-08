using System.Net;
using System.Text.Json;
using Blanquita.Domain.Exceptions;
using FluentValidation;

namespace Blanquita.Web.Middleware;

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
        _logger.LogError(exception, "An unhandled exception has occurred.");

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Se produjeron errores de validación.",
                validationException.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            EntityNotFoundException entityNotFoundException => (
                HttpStatusCode.NotFound,
                entityNotFoundException.Message,
                null
            ),
            DuplicateEntityException duplicateEntityException => (
                HttpStatusCode.Conflict,
                duplicateEntityException.Message,
                null
            ),
            InvalidOperationException invalidOperationException => (
                HttpStatusCode.BadRequest,
                invalidOperationException.Message,
                null
            ),
            BusinessRuleViolationException businessRuleViolationException => (
                HttpStatusCode.BadRequest,
                businessRuleViolationException.Message,
                null
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "No tiene autorización para realizar esta acción.",
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Se produjo un error inesperado en el servidor.",
                null
            )
        };

        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = response.StatusCode,
            Message = message,
            Errors = errors,
            Detail = statusCode == HttpStatusCode.InternalServerError ? null : exception.Message
        });

        await response.WriteAsync(result);
    }
}
