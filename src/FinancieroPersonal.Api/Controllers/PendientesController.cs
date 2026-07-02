using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancieroPersonal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/pendientes")]
public class PendientesController(PeriodoService service) : ControllerBase
{
    /// <summary>Gastos pendientes por pagar (queda &gt; 0) de todos los meses, con el mes de origen.</summary>
    [HttpGet]
    public async Task<ActionResult<List<PendienteGastoDto>>> Get(CancellationToken ct)
        => Ok(await service.PendientesGastosAsync(ct));
}
