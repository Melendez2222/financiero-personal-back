using FinancieroPersonal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Categoria> Categorias { get; }
    DbSet<Periodo> Periodos { get; }
    DbSet<PeriodoCategoria> PeriodoCategorias { get; }
    DbSet<Movimiento> Movimientos { get; }
    DbSet<MetaAhorro> Metas { get; }
    DbSet<AporteMeta> Aportes { get; }
    DbSet<Configuracion> Configuraciones { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
