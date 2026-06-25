using System.Security.Claims;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req, CancellationToken ct)
        => Ok(await service.LoginAsync(req, ct));

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req, CancellationToken ct)
        => StatusCode(201, await service.RegisterAsync(req, ct));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioDto>> Me(CancellationToken ct)
    {
        return Ok(await service.MeAsync(UsuarioId(), ct));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UsuarioDto>> ActualizarPerfil(ActualizarPerfilRequest req, CancellationToken ct)
    {
        return Ok(await service.ActualizarPerfilAsync(UsuarioId(), req, ct));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req, CancellationToken ct)
    {
        await service.ChangePasswordAsync(UsuarioId(), req, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req, CancellationToken ct)
    {
        await service.ResetPasswordAsync(req, ct);
        return NoContent();
    }

    private Guid UsuarioId()
    {
        var sub = User.FindFirstValue("sub");
        if (sub is null || !Guid.TryParse(sub, out var id)) throw AppException.Unauthorized();
        return id;
    }
}
