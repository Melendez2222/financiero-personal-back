using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController(UsuarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> List(CancellationToken ct)
        => Ok(await service.ListAsync(ct));
}
