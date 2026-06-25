using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class ConfiguracionService(IAppDbContext db)
{
    public async Task<ConfiguracionDto> GetAsync(CancellationToken ct)
    {
        var c = await db.Configuraciones.FirstOrDefaultAsync(ct);
        return (c ?? new Configuracion()).ToDto();
    }

    public async Task<ConfiguracionDto> UpdateAsync(ConfiguracionDto dto, CancellationToken ct)
    {
        var c = await db.Configuraciones.FirstOrDefaultAsync(ct);
        if (c is null)
        {
            c = new Configuracion();
            db.Configuraciones.Add(c);
        }
        c.Moneda = dto.Moneda;
        c.Simbolo = dto.Simbolo;
        c.Locale = dto.Locale;
        c.Decimales = dto.Decimales;
        await db.SaveChangesAsync(ct);
        return c.ToDto();
    }
}
