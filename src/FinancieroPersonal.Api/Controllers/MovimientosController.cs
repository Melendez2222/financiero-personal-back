using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using FinancieroPersonal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MovimientosController(MovimientoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MovimientoDto>>> List(
        [FromQuery] Guid? periodoId,
        [FromQuery] Tipo? tipo,
        [FromQuery] Guid? categoriaId,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] string? q,
        CancellationToken ct)
        => Ok(await service.ListAsync(periodoId, tipo, categoriaId, desde, hasta, q, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovimientoDto>> Get(Guid id, CancellationToken ct)
        => Ok(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> Create(CrearMovimientoRequest req, CancellationToken ct)
        => StatusCode(201, await service.CrearAsync(req, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MovimientoDto>> Update(Guid id, ActualizarMovimientoRequest req, CancellationToken ct)
        => Ok(await service.ActualizarAsync(id, req, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }
}
