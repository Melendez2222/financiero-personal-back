using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class AuthService(IAppDbContext db, IPasswordHasher hasher, IJwtTokenService jwt)
{
    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
        if (usuario is null || !hasher.Verify(req.Password, usuario.PasswordHash))
            throw new AppException("credenciales_invalidas", "Correo o contraseña incorrectos.", 401);

        return new AuthResponse(jwt.GenerarToken(usuario), usuario.ToDto());
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var existe = await db.Usuarios.AnyAsync(u => u.Email == req.Email, ct);
        if (existe) throw AppException.Conflict("email_en_uso", "Ese correo ya está registrado.");

        var usuario = new Usuario
        {
            Email = req.Email,
            Nombre = req.Nombre,
            PasswordHash = hasher.Hash(req.Password),
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(jwt.GenerarToken(usuario), usuario.ToDto());
    }

    public async Task<UsuarioDto> MeAsync(Guid usuarioId, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw AppException.Unauthorized();
        return usuario.ToDto();
    }

    /// <summary>Actualiza los datos personales (nombre, apellidos, correo) del usuario autenticado.</summary>
    public async Task<UsuarioDto> ActualizarPerfilAsync(Guid usuarioId, ActualizarPerfilRequest req, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw AppException.Unauthorized();

        var nombre = req.Nombre?.Trim() ?? string.Empty;
        if (nombre.Length == 0)
            throw AppException.BadRequest("nombre_requerido", "El nombre es obligatorio.");

        var email = req.Email?.Trim() ?? string.Empty;
        if (email.Length == 0)
            throw AppException.BadRequest("email_requerido", "El correo es obligatorio.");

        if (!string.Equals(email, usuario.Email, StringComparison.OrdinalIgnoreCase))
        {
            var enUso = await db.Usuarios.AnyAsync(u => u.Email == email && u.Id != usuarioId, ct);
            if (enUso) throw AppException.Conflict("email_en_uso", "Ese correo ya está registrado.");
        }

        usuario.Nombre = nombre;
        usuario.Apellidos = req.Apellidos?.Trim() ?? string.Empty;
        usuario.Email = email;
        await db.SaveChangesAsync(ct);

        return usuario.ToDto();
    }

    /// <summary>Cambio de contraseña del usuario autenticado (verifica la actual).</summary>
    public async Task ChangePasswordAsync(Guid usuarioId, ChangePasswordRequest req, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw AppException.Unauthorized();
        if (!hasher.Verify(req.CurrentPassword, usuario.PasswordHash))
            throw AppException.BadRequest("clave_incorrecta", "La contraseña actual es incorrecta.");
        usuario.PasswordHash = hasher.Hash(req.NewPassword);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Reseteo directo de contraseña por email (sin verificación por correo, uso privado).</summary>
    public async Task ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email, ct)
            ?? throw new AppException("email_no_encontrado", "No existe una cuenta con ese correo.", 404);
        usuario.PasswordHash = hasher.Hash(req.NewPassword);
        await db.SaveChangesAsync(ct);
    }
}
