using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class MetaService(IAppDbContext db)
{
    public async Task<List<MetaDto>> ListAsync(CancellationToken ct)
    {
        var metas = await db.Metas.ToListAsync(ct);
        return metas.Select(m => m.ToDto()).ToList();
    }

    public async Task<MetaDto> CrearAsync(CrearMetaRequest req, CancellationToken ct)
    {
        var estado = req.Estado ?? EstadoMeta.NoIniciado;
        var meta = new MetaAhorro
        {
            Nombre = req.Nombre,
            Emoji = req.Emoji,
            MontoObjetivo = req.MontoObjetivo,
            AporteMensual = req.AporteMensual,
            MontoAcumulado = 0,
            AporteMes = 0,
            FechaLimite = req.FechaLimite,
            Estado = estado,
            Activo = estado == EstadoMeta.Iniciado,
        };
        db.Metas.Add(meta);
        await db.SaveChangesAsync(ct);
        return meta.ToDto();
    }

    public async Task<MetaDto> ActualizarAsync(Guid id, ActualizarMetaRequest req, CancellationToken ct)
    {
        var meta = await db.Metas.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Meta no encontrada.");

        if (req.Nombre is not null) meta.Nombre = req.Nombre;
        if (req.Emoji is not null) meta.Emoji = req.Emoji;
        if (req.MontoObjetivo is not null) meta.MontoObjetivo = req.MontoObjetivo.Value;
        if (req.AporteMensual is not null) meta.AporteMensual = req.AporteMensual.Value;
        if (req.FechaLimite is not null) meta.FechaLimite = req.FechaLimite;
        if (req.Estado is not null) meta.Estado = req.Estado.Value;
        if (req.Activo is not null) meta.Activo = req.Activo.Value;

        await db.SaveChangesAsync(ct);
        return meta.ToDto();
    }

    public async Task<MetaDto> SetActivoAsync(Guid id, bool activo, CancellationToken ct)
    {
        var meta = await db.Metas.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Meta no encontrada.");

        meta.Activo = activo;
        if (meta.Estado != EstadoMeta.Finalizado)
            meta.Estado = activo ? EstadoMeta.Iniciado : EstadoMeta.Suspendido;

        await db.SaveChangesAsync(ct);
        return meta.ToDto();
    }

    public async Task<MetaDto> AportarAsync(Guid id, AporteMetaRequest req, CancellationToken ct)
    {
        var meta = await db.Metas.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Meta no encontrada.");

        meta.MontoAcumulado += req.Monto;
        meta.AporteMes += req.Monto;
        if (meta.MontoAcumulado >= meta.MontoObjetivo)
            meta.Estado = EstadoMeta.Finalizado;

        db.Aportes.Add(new AporteMeta
        {
            MetaId = id,
            Monto = req.Monto,
            Fecha = req.Fecha,
            Descripcion = req.Descripcion,
        });

        await db.SaveChangesAsync(ct);
        return meta.ToDto();
    }

    public async Task<List<AporteDto>> ListAportesAsync(Guid metaId, CancellationToken ct)
    {
        var aportes = await db.Aportes.Where(a => a.MetaId == metaId).ToListAsync(ct);
        return aportes
            .OrderByDescending(a => a.Fecha)
            .Select(a => new AporteDto(a.Id, a.Monto, a.Fecha, a.Descripcion))
            .ToList();
    }
}
