using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class DeudaService(IAppDbContext db)
{
    public async Task<List<DeudaDto>> ListAsync(CancellationToken ct)
    {
        var deudas = await db.Categorias
            .Where(c => c.Tipo == Tipo.Deuda)
            .OrderBy(c => c.Orden)
            .ToListAsync(ct);

        // El saldo baja solo por el CAPITAL de cada abono (MontoCapital); el resto es interés.
        // MontoCapital null = el abono completo es capital (deudas sin interés / back-compat).
        var statsPorCat = (await db.Movimientos
                .Where(m => m.Tipo == Tipo.Deuda && m.CategoriaId != null)
                .ToListAsync(ct))
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(
                g => g.Key,
                g => (
                    Capital: g.Sum(m => m.MontoCapital ?? m.Monto),
                    Interes: g.Sum(m => m.Monto - (m.MontoCapital ?? m.Monto)),
                    // Cuotas pagadas = abonos marcados como cuota regular (los extra no cuentan).
                    Cuotas: g.Count(m => m.EsCuota)));

        return deudas.Select(c =>
        {
            var stats = statsPorCat.GetValueOrDefault(c.Id);
            var capitalPagado = Calc.Round2(stats.Capital);
            var interesPagado = Calc.Round2(stats.Interes);
            decimal? saldo = c.MontoTotal is { } total ? Calc.Round2(Math.Max(0, total - capitalPagado)) : null;
            decimal? pct = c.MontoTotal is { } t && t > 0 ? Calc.Round2(Math.Min(100, (capitalPagado / t) * 100)) : null;
            return new DeudaDto(
                c.Id, c.Nombre, c.Emoji, c.FechaVencimiento, c.Presupuesto, c.CuotasRestantes,
                c.MontoTotal, c.CapitalPorCuota, c.TipoDeuda, capitalPagado, interesPagado, stats.Cuotas,
                saldo, pct, c.Activo);
        }).ToList();
    }
}
