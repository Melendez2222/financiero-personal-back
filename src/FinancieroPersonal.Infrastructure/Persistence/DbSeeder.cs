using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Infrastructure.Persistence;

/// <summary>Siembra solo lo imprescindible: los 2 usuarios reales y la configuración. Sin data demo.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        await SeedUsuariosAsync(db, hasher, ct);
        await AsegurarConfiguracionAsync(db, ct);
    }

    private static async Task SeedUsuariosAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct)
    {
        // Elimina los usuarios demo antiguos si existen.
        var demo = await db.Usuarios
            .Where(u => u.Email == "ana@correo.com" || u.Email == "luis@correo.com")
            .ToListAsync(ct);
        if (demo.Count > 0) db.Usuarios.RemoveRange(demo);

        var reales = new[]
        {
            ("cristhian.melendez2711@gmail.com", "Cristhian", "Meléndez"),
            ("nicoleallisson2000@gmail.com", "Nicole", "Allisson"),
        };
        foreach (var (email, nombre, apellidos) in reales)
        {
            if (!await db.Usuarios.AnyAsync(u => u.Email == email, ct))
            {
                db.Usuarios.Add(new Usuario
                {
                    Email = email,
                    Nombre = nombre,
                    Apellidos = apellidos,
                    PasswordHash = hasher.Hash("Test2026!"),
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task AsegurarConfiguracionAsync(AppDbContext db, CancellationToken ct)
    {
        if (!await db.Configuraciones.AnyAsync(ct))
        {
            db.Configuraciones.Add(new Configuracion());
            await db.SaveChangesAsync(ct);
        }
    }
}
