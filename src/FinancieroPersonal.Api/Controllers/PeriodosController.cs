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
    public async Task<ActionResult<ResumenPeriodoDto>> Resumen(Guid id, [FromQuery] Guid? usuarioId, CancellationToken ct)
        => Ok(await service.ResumenAsync(id, usuarioId, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }

    /// <summary>Marca una categoría como "cumplida" en el periodo (con justificación opcional).</summary>
    [HttpPost("{id:guid}/cierres")]
    public async Task<IActionResult> MarcarCumplido(Guid id, CrearCierreRequest req, CancellationToken ct)
    {
        await service.MarcarCumplidoAsync(id, req.CategoriaId, req.Justificacion, ct);
        return NoContent();
    }

    /// <summary>Reabre una categoría cumplida (vuelve a contar como pendiente).</summary>
    [HttpDelete("{id:guid}/cierres/{categoriaId:guid}")]
    public async Task<IActionResult> ReabrirCumplido(Guid id, Guid categoriaId, CancellationToken ct)
    {
        await service.ReabrirCumplidoAsync(id, categoriaId, ct);
        return NoContent();
    }

    /// <summary>
    /// Barrido: apaga (Activo=false) las categorías de vigencia acotada ya cumplidas en su último mes.
    /// Idempotente; se llama al cargar la app para regularizar datos preexistentes. Devuelve cuántas se apagaron.
    /// </summary>
    [HttpPost("autodesactivar")]
    public async Task<ActionResult<object>> Autodesactivar(CancellationToken ct)
        => Ok(new { apagadas = await service.AutodesactivarCumplidasAsync(ct) });
}
