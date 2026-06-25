using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MetasController(MetaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MetaDto>>> List([FromQuery] Guid? periodoId, CancellationToken ct)
        => Ok(await service.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<MetaDto>> Create(CrearMetaRequest req, CancellationToken ct)
        => StatusCode(201, await service.CrearAsync(req, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MetaDto>> Update(Guid id, ActualizarMetaRequest req, CancellationToken ct)
        => Ok(await service.ActualizarAsync(id, req, ct));

    [HttpPatch("{id:guid}/activo")]
    public async Task<ActionResult<MetaDto>> SetActivo(Guid id, SetActivoRequest req, CancellationToken ct)
        => Ok(await service.SetActivoAsync(id, req.Activo, ct));

    [HttpPost("{id:guid}/aportes")]
    public async Task<ActionResult<MetaDto>> Aportar(Guid id, AporteMetaRequest req, CancellationToken ct)
        => StatusCode(201, await service.AportarAsync(id, req, ct));

    [HttpGet("{id:guid}/aportes")]
    public async Task<ActionResult<List<AporteDto>>> Aportes(Guid id, CancellationToken ct)
        => Ok(await service.ListAportesAsync(id, ct));
}
