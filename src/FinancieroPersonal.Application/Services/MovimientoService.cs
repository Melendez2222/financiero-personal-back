using System.Globalization;
using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class MovimientoService(IAppDbContext db)
{
    public async Task<List<MovimientoDto>> ListAsync(
        Guid? periodoId, Tipo? tipo, Guid? categoriaId, DateOnly? desde, DateOnly? hasta, string? q,
        CancellationToken ct)
    {
        var query = db.Movimientos.AsQueryable();
        if (periodoId is not null) query = query.Where(m => m.PeriodoId == periodoId);
        if (tipo is not null) query = query.Where(m => m.Tipo == tipo);
        if (categoriaId is not null) query = query.Where(m => m.CategoriaId == categoriaId);
        if (desde is not null) query = query.Where(m => m.Fecha >= desde);
        if (hasta is not null) query = query.Where(m => m.Fecha <= hasta);

        var list = await query.ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(q))
        {
            list = list.Where(m =>
                m.Nota.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.Monto.ToString(CultureInfo.InvariantCulture).Contains(q)).ToList();
        }

        return list
            .OrderByDescending(m => m.Fecha)
            .Select(m => m.ToDto())
            .ToList();
    }

    public async Task<MovimientoDto> GetAsync(Guid id, CancellationToken ct)
    {
        var m = await db.Movimientos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Movimiento no encontrado.");
        return m.ToDto();
    }

    public async Task<MovimientoDto> CrearAsync(CrearMovimientoRequest req, CancellationToken ct)
    {
        var movimiento = new Movimiento
        {
            PeriodoId = req.PeriodoId,
            CategoriaId = req.Tipo == Tipo.Situacional ? null : req.CategoriaId,
            Concepto = req.Tipo == Tipo.Situacional ? req.Concepto : null,
            Tipo = req.Tipo,
            UsuarioId = req.UsuarioId,
            Fecha = req.Fecha,
            Monto = req.Monto,
            // Solo las deudas guardan el desglose capital/interés; el resto, null.
            MontoCapital = req.Tipo == Tipo.Deuda ? req.MontoCapital : null,
            EsCuota = req.EsCuota ?? true,
            Nota = req.Nota ?? string.Empty,
        };
        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return movimiento.ToDto();
    }

    public async Task<MovimientoDto> ActualizarAsync(Guid id, ActualizarMovimientoRequest req, CancellationToken ct)
    {
        var m = await db.Movimientos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Movimiento no encontrado.");

        if (req.Tipo is not null) m.Tipo = req.Tipo.Value;
        if (req.Fecha is not null) m.Fecha = req.Fecha.Value;
        if (req.Monto is not null) m.Monto = req.Monto.Value;
        if (req.Nota is not null) m.Nota = req.Nota;
        m.UsuarioId = req.UsuarioId;
        // Solo deudas conservan el desglose capital/interés.
        m.MontoCapital = m.Tipo == Tipo.Deuda ? req.MontoCapital : null;
        if (req.EsCuota is not null) m.EsCuota = req.EsCuota.Value;

        if (m.Tipo == Tipo.Situacional)
        {
            m.CategoriaId = null;
            if (req.Concepto is not null) m.Concepto = req.Concepto;
        }
        else
        {
            m.Concepto = null;
            if (req.CategoriaId is not null) m.CategoriaId = req.CategoriaId.Value;
        }

        await db.SaveChangesAsync(ct);
        return m.ToDto();
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct)
    {
        var m = await db.Movimientos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return;
        m.Eliminado = true;
        m.EliminadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
