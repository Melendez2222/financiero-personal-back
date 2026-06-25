using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConfiguracionController(ConfiguracionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ConfiguracionDto>> Get(CancellationToken ct)
        => Ok(await service.GetAsync(ct));

    [HttpPut]
    public async Task<ActionResult<ConfiguracionDto>> Update(ConfiguracionDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(dto, ct));
}
