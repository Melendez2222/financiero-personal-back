using System.Globalization;
using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class MovimientoService(IAppDbContext db, PeriodoService periodos)
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

    private async Task<MetaAhorro> ValidarYCargarAhorroAsync(Guid metaId, decimal monto, CancellationToken ct)
    {
        var meta = await db.Metas.FirstOrDefaultAsync(x => x.Id == metaId, ct)
            ?? throw AppException.BadRequest("ahorro_invalido", "Ahorro no encontrado.");
        if (meta.MontoObjetivo is not null)
            throw AppException.BadRequest("ahorro_invalido", "Solo se puede pagar desde un ahorro (sin meta fija).");
        if (meta.MontoAcumulado < monto)
            throw AppException.BadRequest("saldo_insuficiente",
                $"Saldo insuficiente en el ahorro (disponible: {meta.MontoAcumulado:N2}).");
        return meta;
    }

    private async Task RevertirCargoAhorroAsync(Guid? metaId, decimal monto, CancellationToken ct)
    {
        if (metaId is null) return;
        var meta = await db.Metas.FirstOrDefaultAsync(x => x.Id == metaId, ct);
        if (meta is not null) meta.MontoAcumulado += monto;
    }

    public async Task<MovimientoDto> CrearAsync(CrearMovimientoRequest req, CancellationToken ct)
    {
        if (req.MetaId is not null && req.Tipo == Tipo.Ingreso)
            throw AppException.BadRequest("ahorro_invalido", "Los ingresos no pueden pagarse desde un ahorro.");

        var sinCategoria = req.Tipo == Tipo.Situacional
            || (req.Tipo == Tipo.Ingreso && req.CategoriaId is null);

        MetaAhorro? meta = null;
        if (req.MetaId is not null)
            meta = await ValidarYCargarAhorroAsync(req.MetaId.Value, req.Monto, ct);

        var movimiento = new Movimiento
        {
            PeriodoId = req.PeriodoId,
            CategoriaId = sinCategoria ? null : req.CategoriaId,
            Concepto = sinCategoria ? req.Concepto : null,
            Tipo = req.Tipo,
            UsuarioId = req.UsuarioId,
            Fecha = req.Fecha,
            Monto = req.Monto,
            MontoCapital = req.Tipo == Tipo.Deuda ? req.MontoCapital : null,
            EsCuota = req.EsCuota ?? true,
            Nota = req.Nota ?? string.Empty,
            Cobertura = req.Cobertura,
            MetaId = req.MetaId,
        };

        if (meta is not null)
            meta.MontoAcumulado -= req.Monto;

        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync(ct);

        // "Apagarla apenas es abonada": si este pago dejó la categoría cubierta en su último mes de
        // vigencia, se auto-desactiva en Configuración.
        if (movimiento.CategoriaId is not null)
            await periodos.EvaluarAutoDesactivacionAsync(movimiento.PeriodoId, movimiento.CategoriaId.Value, ct);

        return movimiento.ToDto();
    }

    public async Task<MovimientoDto> ActualizarAsync(Guid id, ActualizarMovimientoRequest req, CancellationToken ct)
    {
        var m = await db.Movimientos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Movimiento no encontrado.");

        var metaIdAnterior = m.MetaId;
        var montoAnterior = m.Monto;

        await RevertirCargoAhorroAsync(metaIdAnterior, montoAnterior, ct);

        if (req.Tipo is not null) m.Tipo = req.Tipo.Value;
        if (req.Fecha is not null) m.Fecha = req.Fecha.Value;
        if (req.Monto is not null) m.Monto = req.Monto.Value;
        if (req.Nota is not null) m.Nota = req.Nota;
        m.UsuarioId = req.UsuarioId;
        m.Cobertura = req.Cobertura;
        m.MetaId = req.MetaId;

        if (m.Tipo == Tipo.Ingreso && m.MetaId is not null)
            throw AppException.BadRequest("ahorro_invalido", "Los ingresos no pueden pagarse desde un ahorro.");

        if (m.Tipo == Tipo.Deuda) { if (req.MontoCapital is not null) m.MontoCapital = req.MontoCapital; }
        else { m.MontoCapital = null; }
        if (req.EsCuota is not null) m.EsCuota = req.EsCuota.Value;

        if (m.Tipo == Tipo.Situacional)
        {
            m.CategoriaId = null;
            if (req.Concepto is not null) m.Concepto = req.Concepto;
        }
        else if (req.CategoriaId is not null)
        {
            m.CategoriaId = req.CategoriaId.Value;
            m.Concepto = null;
        }
        else if (m.Tipo == Tipo.Ingreso && m.CategoriaId is null)
        {
            if (req.Concepto is not null) m.Concepto = req.Concepto;
        }

        if (m.MetaId is not null)
        {
            var meta = await ValidarYCargarAhorroAsync(m.MetaId.Value, m.Monto, ct);
            meta.MontoAcumulado -= m.Monto;
        }

        await db.SaveChangesAsync(ct);

        if (m.CategoriaId is not null)
            await periodos.EvaluarAutoDesactivacionAsync(m.PeriodoId, m.CategoriaId.Value, ct);

        return m.ToDto();
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct)
    {
        var m = await db.Movimientos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return;

        await RevertirCargoAhorroAsync(m.MetaId, m.Monto, ct);

        m.Eliminado = true;
        m.EliminadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
