using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class CategoriaService(IAppDbContext db)
{
    public async Task<List<CategoriaDto>> ListAsync(Tipo? tipo, CancellationToken ct)
    {
        var query = db.Categorias.AsQueryable();
        if (tipo is not null) query = query.Where(c => c.Tipo == tipo);
        var list = await query.OrderBy(c => c.Orden).ToListAsync(ct);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoriaDto> GetAsync(Guid id, CancellationToken ct)
    {
        var c = await db.Categorias.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Categoría no encontrada.");
        return c.ToDto();
    }

    public async Task<CategoriaDto> CrearAsync(CrearCategoriaRequest req, CancellationToken ct)
    {
        var ordenMax = await db.Categorias
            .Where(c => c.Tipo == req.Tipo)
            .Select(c => (int?)c.Orden)
            .MaxAsync(ct) ?? 0;

        var categoria = new Categoria
        {
            Nombre = req.Nombre,
            Tipo = req.Tipo,
            Presupuesto = req.Presupuesto,
            Emoji = req.Emoji,
            FechaVencimiento = req.FechaVencimiento,
            CuotasRestantes = req.CuotasRestantes,
            MontoTotal = req.MontoTotal,
            CapitalPorCuota = req.CapitalPorCuota,
            TipoDeuda = req.TipoDeuda,
            Activo = req.Activo ?? true,
            Orden = ordenMax + 1,
        };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(ct);
        return categoria.ToDto();
    }

    public async Task<CategoriaDto> ActualizarAsync(Guid id, ActualizarCategoriaRequest req, CancellationToken ct)
    {
        var c = await db.Categorias.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Categoría no encontrada.");

        if (req.Nombre is not null) c.Nombre = req.Nombre;
        if (req.Presupuesto is not null) c.Presupuesto = req.Presupuesto.Value;
        if (req.Emoji is not null) c.Emoji = req.Emoji;
        if (req.FechaVencimiento is not null) c.FechaVencimiento = req.FechaVencimiento;
        if (req.CuotasRestantes is not null) c.CuotasRestantes = req.CuotasRestantes;
        if (req.MontoTotal is not null) c.MontoTotal = req.MontoTotal;
        // Asignación directa para permitir desactivar el interés (capitalPorCuota = null).
        // El diálogo de categoría siempre envía el objeto completo.
        c.CapitalPorCuota = req.CapitalPorCuota;
        c.TipoDeuda = req.TipoDeuda;
        if (req.Activo is not null) c.Activo = req.Activo.Value;

        await db.SaveChangesAsync(ct);
        return c.ToDto();
    }

    public async Task<CategoriaDto> SetActivoAsync(Guid id, bool activo, CancellationToken ct)
    {
        var c = await db.Categorias.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Categoría no encontrada.");
        c.Activo = activo;
        await db.SaveChangesAsync(ct);
        return c.ToDto();
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct)
    {
        var c = await db.Categorias.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Categoría no encontrada.");

        var tieneMovs = await db.Movimientos.AnyAsync(m => m.CategoriaId == id, ct);
        if (tieneMovs)
            throw AppException.Conflict("tiene_movimientos",
                "No se puede eliminar: tiene movimientos. Mejor desactívala.");

        // Borrado lógico: el snapshot (PeriodoCategoria) se conserva; el filtro global de
        // Categoria ya excluye esta categoría de los resúmenes.
        c.Eliminado = true;
        c.EliminadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
