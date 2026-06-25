using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using FinancieroPersonal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController(CategoriaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoriaDto>>> List([FromQuery] Tipo? tipo, CancellationToken ct)
        => Ok(await service.ListAsync(tipo, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoriaDto>> Get(Guid id, CancellationToken ct)
        => Ok(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create(CrearCategoriaRequest req, CancellationToken ct)
        => StatusCode(201, await service.CrearAsync(req, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoriaDto>> Update(Guid id, ActualizarCategoriaRequest req, CancellationToken ct)
        => Ok(await service.ActualizarAsync(id, req, ct));

    [HttpPatch("{id:guid}/activo")]
    public async Task<ActionResult<CategoriaDto>> SetActivo(Guid id, SetActivoRequest req, CancellationToken ct)
        => Ok(await service.SetActivoAsync(id, req.Activo, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }
}
