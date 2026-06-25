using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PeriodosController(PeriodoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PeriodoDto>>> List(CancellationToken ct)
        => Ok(await service.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PeriodoDto>> Get(Guid id, CancellationToken ct)
        => Ok(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<PeriodoDto>> Create(CrearPeriodoRequest req, CancellationToken ct)
        => StatusCode(201, await service.CrearAsync(req, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PeriodoDto>> Update(Guid id, ActualizarPeriodoRequest req, CancellationToken ct)
        => Ok(await service.ActualizarAsync(id, req, ct));

    [HttpPost("{id:guid}/iniciar")]
    public async Task<ActionResult<PeriodoDto>> Iniciar(Guid id, CancellationToken ct)
        => Ok(await service.IniciarAsync(id, ct));

    [HttpGet("{id:guid}/resumen")]
    public async Task<ActionResult<ResumenPeriodoDto>> Resumen(Guid id, CancellationToken ct)
        => Ok(await service.ResumenAsync(id, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }
}
