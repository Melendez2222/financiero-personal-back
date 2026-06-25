using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class UsuarioService(IAppDbContext db)
{
    public async Task<List<UsuarioDto>> ListAsync(CancellationToken ct)
    {
        var usuarios = await db.Usuarios.OrderBy(u => u.Nombre).ToListAsync(ct);
        return usuarios.Select(u => u.ToDto()).ToList();
    }
}
