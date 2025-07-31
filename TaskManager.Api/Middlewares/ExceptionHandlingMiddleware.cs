using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.Status;

            var result = new
            {
                error = ex.Message
            };
            await context.Response.WriteAsJsonAsync(result);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = new
            {
                error = "An unexpected error occurred."
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}