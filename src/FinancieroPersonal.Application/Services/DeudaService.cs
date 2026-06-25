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

        var pagosPorCat = (await db.Movimientos
                .Where(m => m.Tipo == Tipo.Deuda && m.CategoriaId != null)
                .ToListAsync(ct))
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto));

        return deudas.Select(c =>
        {
            var pagado = Calc.Round2(pagosPorCat.GetValueOrDefault(c.Id));
            decimal? saldo = c.MontoTotal is { } total ? Calc.Round2(Math.Max(0, total - pagado)) : null;
            decimal? pct = c.MontoTotal is { } t && t > 0 ? Calc.Round2(Math.Min(100, (pagado / t) * 100)) : null;
            return new DeudaDto(
                c.Id, c.Nombre, c.Emoji, c.FechaVencimiento, c.Presupuesto, c.CuotasRestantes,
                c.MontoTotal, pagado, saldo, pct, c.Activo);
        }).ToList();
    }
}
