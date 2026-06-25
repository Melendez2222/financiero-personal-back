namespace FinancieroPersonal.Application.Common;

/// <summary>Excepción de negocio que el middleware traduce a { code, message } + status HTTP.</summary>
public class AppException(string code, string message, int statusCode) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;

    public static AppException NotFound(string message = "Recurso no encontrado.") =>
        new("no_encontrado", message, 404);

    public static AppException Conflict(string code, string message) => new(code, message, 409);

    public static AppException Unauthorized(string message = "Sesión inválida.") =>
        new("no_autenticado", message, 401);

    public static AppException BadRequest(string code, string message) => new(code, message, 400);
}
