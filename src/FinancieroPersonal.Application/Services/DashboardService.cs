using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class DashboardService(IAppDbContext db, PeriodoService periodos)
{
    public async Task<DashboardDto> GetAsync(Guid? periodoId, CancellationToken ct)
    {
        var todos = await db.Periodos.ToListAsync(ct);
        if (todos.Count == 0) throw AppException.NotFound("Sin datos de dashboard.");

        var ordenados = todos.OrderBy(p => p.Anio).ThenBy(p => p.Mes).ToList();
        var periodo = periodoId is null
            ? ordenados[^1]
            : ordenados.FirstOrDefault(p => p.Id == periodoId)
              ?? throw AppException.NotFound("Periodo no encontrado.");

        var idx = ordenados.FindIndex(p => p.Id == periodo.Id);
        var previo = idx > 0 ? ordenados[idx - 1] : null;

        var movs = await db.Movimientos.ToListAsync(ct);
        var categorias = await db.Categorias.ToListAsync(ct);
        var metas = await db.Metas.ToListAsync(ct);

        decimal TotalTipo(Guid pid, Tipo t) =>
            Calc.Round2(movs.Where(m => m.PeriodoId == pid && m.Tipo == t).Sum(m => m.Monto));

        decimal Delta(decimal actual, decimal anterior) =>
            anterior == 0 ? (actual == 0 ? 0 : 100) : Calc.Round2((actual - anterior) / anterior * 100);

        KpiValorDto Kpi(Tipo t)
        {
            var actual = TotalTipo(periodo.Id, t);
            var anterior = previo is null ? 0 : TotalTipo(previo.Id, t);
            return new KpiValorDto(actual, Delta(actual, anterior));
        }

        var flujoMeses = ordenados.Select(p => new FlujoMesDto(
            Calc.MesesAbbr[p.Mes - 1],
            TotalTipo(p.Id, Tipo.Ingreso),
            Calc.Round2(Calc.TiposGastoActual.Sum(t => TotalTipo(p.Id, t))))).ToList();

        var actualByCat = movs.Where(m => m.PeriodoId == periodo.Id && m.CategoriaId != null)
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto));

        var gastosCat = categorias
            .Where(c => Calc.EsGasto(c.Tipo) && actualByCat.GetValueOrDefault(c.Id) > 0)
            .Select(c => new { Nombre = c.Nombre, Tipo = c.Tipo, Monto = Calc.Round2(actualByCat[c.Id]) })
            .ToList();

        var situacionales = movs
            .Where(m => m.PeriodoId == periodo.Id && m.Tipo == Tipo.Situacional)
            .Select(m => new
            {
                Nombre = string.IsNullOrWhiteSpace(m.Concepto) ? "Situacional" : m.Concepto!,
                Tipo = Tipo.Situacional,
                Monto = Calc.Round2(m.Monto),
            })
            .ToList();

        var todosGastos = gastosCat.Concat(situacionales).ToList();
        var totalGastos = todosGastos.Sum(g => g.Monto);
        var desglose = todosGastos
            .Select(g => new DesgloseDto(g.Nombre, g.Tipo, g.Monto,
                totalGastos > 0 ? Calc.Round2(g.Monto / totalGastos * 100) : 0))
            .OrderByDescending(d => d.Monto)
            .ToList();

        var resumen = await periodos.ResumenAsync(periodo.Id, ct);

        var metasDto = metas.Select(m => new MetaProgresoDto(
            m.Id, m.Nombre,
            m.MontoObjetivo > 0 ? Math.Min(100, Calc.Round2(m.MontoAcumulado / m.MontoObjetivo * 100)) : 0,
            m.MontoAcumulado, m.MontoObjetivo)).ToList();

        var kpis = new KpisDto(
            Kpi(Tipo.Ingreso), Kpi(Tipo.Fijo), Kpi(Tipo.Necesario), Kpi(Tipo.Deuda), Kpi(Tipo.Ahorro));

        return new DashboardDto(kpis, flujoMeses, desglose, resumen.Disponible, metasDto);
    }
}
