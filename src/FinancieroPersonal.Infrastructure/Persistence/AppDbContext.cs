using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<PeriodoCategoria> PeriodoCategorias => Set<PeriodoCategoria>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<MetaAhorro> Metas => Set<MetaAhorro>();
    public DbSet<AporteMeta> Aportes => Set<AporteMeta>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Usuario>(e =>
        {
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Apellidos).HasMaxLength(120);
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Categoria>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Presupuesto).HasPrecision(18, 2);
            e.Property(x => x.MontoTotal).HasPrecision(18, 2);
            e.Property(x => x.CapitalPorCuota).HasPrecision(18, 2);
            e.Property(x => x.Emoji).HasMaxLength(16);
            e.Property(x => x.FechaVencimiento).HasMaxLength(8);
        });

        b.Entity<Periodo>(e =>
        {
            e.Property(x => x.Moneda).HasMaxLength(8);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.BalanceInicial).HasPrecision(18, 2);
            e.HasIndex(x => new { x.Anio, x.Mes }).IsUnique();
        });

        b.Entity<PeriodoCategoria>(e =>
        {
            e.Property(x => x.MontoPresupuestado).HasPrecision(18, 2);
            e.HasIndex(x => new { x.PeriodoId, x.CategoriaId }).IsUnique();
        });

        b.Entity<Movimiento>(e =>
        {
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Monto).HasPrecision(18, 2);
            e.Property(x => x.MontoCapital).HasPrecision(18, 2);
            e.Property(x => x.Nota).HasMaxLength(500);
            e.Property(x => x.Concepto).HasMaxLength(200);
            e.HasIndex(x => x.PeriodoId);
            e.HasIndex(x => x.CategoriaId);
            e.HasIndex(x => x.UsuarioId);
        });

        b.Entity<MetaAhorro>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Emoji).HasMaxLength(16);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MontoObjetivo).HasPrecision(18, 2);
            e.Property(x => x.AporteMensual).HasPrecision(18, 2);
            e.Property(x => x.MontoAcumulado).HasPrecision(18, 2);
            e.Property(x => x.AporteMes).HasPrecision(18, 2);
        });

        b.Entity<AporteMeta>(e =>
        {
            e.Property(x => x.Monto).HasPrecision(18, 2);
            e.Property(x => x.Descripcion).HasMaxLength(300);
            e.HasIndex(x => x.MetaId);
        });

        b.Entity<Configuracion>(e =>
        {
            e.Property(x => x.Moneda).HasMaxLength(8);
            e.Property(x => x.Simbolo).HasMaxLength(8);
            e.Property(x => x.Locale).HasMaxLength(16);
        });
    }
}
