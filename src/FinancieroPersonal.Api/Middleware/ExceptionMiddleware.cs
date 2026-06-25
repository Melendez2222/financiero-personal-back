using FinancieroPersonal.Application.Common;

namespace FinancieroPersonal.Api.Middleware;

/// <summary>Traduce excepciones a respuestas JSON { code, message } con el status adecuado.</summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { code = ex.Code, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                new { code = "error_interno", message = "Ocurrió un error inesperado." });
        }
    }
}
