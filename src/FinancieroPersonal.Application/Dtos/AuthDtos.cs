namespace FinancieroPersonal.Application.Dtos;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Nombre, string Password);

public record UsuarioDto(Guid Id, string Email, string Nombre, string Apellidos);

public record AuthResponse(string Token, UsuarioDto Usuario);

public record ActualizarPerfilRequest(string Nombre, string Apellidos, string Email);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ResetPasswordRequest(string Email, string NewPassword);
