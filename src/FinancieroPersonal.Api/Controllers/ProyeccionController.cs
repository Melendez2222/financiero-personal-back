using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProyeccionController(ProyeccionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GuiaDto>> Get([FromQuery] int meses, CancellationToken ct)
        => Ok(await service.GetAsync(meses, ct));
}
