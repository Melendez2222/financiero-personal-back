using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class DashboardService(IAppDbContext db, PeriodoService periodos)
{
    public async Task<DashboardDto> GetAsync(Guid? periodoId, Guid? usuarioId, CancellationToken ct)
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

        // Vista por persona: solo los movimientos atribuidos a ella (null = global del hogar).
        var movs = await db.Movimientos
            .Where(m => usuarioId == null || m.UsuarioId == usuarioId)
            .ToListAsync(ct);
        var categorias = await db.Categorias.ToListAsync(ct);
        var metas = await db.Metas.ToListAsync(ct);

        // Para DEUDAS, la "recuperación" es solo el capital (MontoCapital ?? Monto); el interés no
        // cuenta como pago de la deuda. Para el resto de tipos es el monto completo.
        decimal MontoOCapital(Movimiento m) => m.Tipo == Tipo.Deuda ? (m.MontoCapital ?? m.Monto) : m.Monto;

        // Pago completo (efectivo) — para el gráfico de flujo de meses (entradas vs salidas).
        decimal TotalTipo(Guid pid, Tipo t) =>
            Calc.Round2(movs.Where(m => m.PeriodoId == pid && m.Tipo == t).Sum(m => m.Monto));
        // Recuperación (capital en deudas, monto completo en el resto) — para KPIs y desglose.
        decimal TotalRecuperacion(Guid pid, Tipo t) =>
            Calc.Round2(movs.Where(m => m.PeriodoId == pid && m.Tipo == t).Sum(MontoOCapital));

        decimal Delta(decimal actual, decimal anterior) =>
            anterior == 0 ? (actual == 0 ? 0 : 100) : Calc.Round2((actual - anterior) / anterior * 100);

        KpiValorDto Kpi(Tipo t)
        {
            var actual = TotalRecuperacion(periodo.Id, t);
            var anterior = previo is null ? 0 : TotalRecuperacion(previo.Id, t);
            return new KpiValorDto(actual, Delta(actual, anterior));
        }

        var flujoMeses = ordenados.Select(p => new FlujoMesDto(
            Calc.MesesAbbr[p.Mes - 1],
            TotalTipo(p.Id, Tipo.Ingreso),
            Calc.Round2(Calc.TiposGastoActual.Sum(t => TotalTipo(p.Id, t))))).ToList();

        var actualByCat = movs.Where(m => m.PeriodoId == periodo.Id && m.CategoriaId != null)
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(MontoOCapital));

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

        var resumen = await periodos.ResumenAsync(periodo.Id, usuarioId, ct);

        var metasDto = metas.Select(m => new MetaProgresoDto(
            m.Id, m.Nombre,
            m.MontoObjetivo is { } o && o > 0 ? Math.Min(100, Calc.Round2(m.MontoAcumulado / o * 100)) : 0,
            m.MontoAcumulado, m.MontoObjetivo)).ToList();

        var kpis = new KpisDto(
            Kpi(Tipo.Ingreso), Kpi(Tipo.Fijo), Kpi(Tipo.Necesario), Kpi(Tipo.Deuda), Kpi(Tipo.Ahorro));

        return new DashboardDto(kpis, flujoMeses, desglose, resumen.Disponible, metasDto);
    }
}
