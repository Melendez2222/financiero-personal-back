using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Application.Mapping;
using FinancieroPersonal.Domain.Entities;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class PeriodoService(IAppDbContext db)
{
    public async Task<List<PeriodoDto>> ListAsync(CancellationToken ct)
    {
        var periodos = await db.Periodos.ToListAsync(ct);
        return periodos
            .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
            .Select(p => p.ToDto())
            .ToList();
    }

    public async Task<PeriodoDto> GetAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Periodos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Periodo no encontrado.");
        return p.ToDto();
    }

    public async Task<PeriodoDto> CrearAsync(CrearPeriodoRequest req, CancellationToken ct)
    {
        var existe = await db.Periodos.AnyAsync(p => p.Anio == req.Anio && p.Mes == req.Mes, ct);
        if (existe) throw AppException.Conflict("periodo_existe", "Ese mes ya existe.");

        var balanceInicial = req.BalanceInicial ?? 0m;
        if (req.HeredarBalance == true)
        {
            var periodos = await db.Periodos.ToListAsync(ct);
            var previo = periodos
                .Where(p => p.Anio < req.Anio || (p.Anio == req.Anio && p.Mes < req.Mes))
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .FirstOrDefault();
            if (previo is not null) balanceInicial = await BalanceFinalAsync(previo, ct);
        }

        var periodo = new Periodo
        {
            Anio = req.Anio,
            Mes = req.Mes,
            FechaInicio = new DateOnly(req.Anio, req.Mes, 1),
            FechaFin = new DateOnly(req.Anio, req.Mes, DateTime.DaysInMonth(req.Anio, req.Mes)),
            Moneda = req.Moneda ?? "PEN",
            BalanceInicial = balanceInicial,
            Estado = EstadoPeriodo.Borrador,
        };
        db.Periodos.Add(periodo);
        await AgregarSnapshotAsync(periodo.Id, ct);
        await db.SaveChangesAsync(ct);
        return periodo.ToDto();
    }

    public async Task<PeriodoDto> ActualizarAsync(Guid id, ActualizarPeriodoRequest req, CancellationToken ct)
    {
        var p = await db.Periodos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Periodo no encontrado.");

        if (req.BalanceInicial is not null) p.BalanceInicial = req.BalanceInicial.Value;
        if (req.Moneda is not null) p.Moneda = req.Moneda;
        if (req.Estado is not null) p.Estado = req.Estado.Value;

        await db.SaveChangesAsync(ct);
        return p.ToDto();
    }

    public async Task<PeriodoDto> IniciarAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Periodos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Periodo no encontrado.");

        await AgregarSnapshotAsync(p.Id, ct);
        p.Estado = EstadoPeriodo.Iniciado;
        await db.SaveChangesAsync(ct);
        return p.ToDto();
    }

    public async Task<ResumenPeriodoDto> ResumenAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Periodos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Periodo no encontrado.");
        return await ConstruirResumenAsync(p, ct);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Periodos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Periodo no encontrado.");

        var tieneMovs = await db.Movimientos.AnyAsync(m => m.PeriodoId == id, ct);
        if (tieneMovs)
            throw AppException.Conflict("periodo_con_movimientos",
                "El mes tiene movimientos y no se puede borrar.");

        // Borrado lógico: el snapshot (PeriodoCategoria) se conserva para auditoría; al recrear
        // el mes se genera un periodo nuevo (otro Id) con su propio snapshot.
        p.Eliminado = true;
        p.EliminadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Crea el snapshot de categorías activas para un periodo (si aún no existe).</summary>
    private async Task AgregarSnapshotAsync(Guid periodoId, CancellationToken ct)
    {
        var yaTiene = await db.PeriodoCategorias.AnyAsync(pc => pc.PeriodoId == periodoId, ct);
        if (yaTiene) return;

        var activas = await db.Categorias.Where(c => c.Activo).ToListAsync(ct);
        foreach (var c in activas)
        {
            db.PeriodoCategorias.Add(new PeriodoCategoria
            {
                PeriodoId = periodoId,
                CategoriaId = c.Id,
                MontoPresupuestado = c.Presupuesto,
            });
        }
    }

    private async Task<decimal> BalanceFinalAsync(Periodo periodo, CancellationToken ct)
    {
        var r = await ConstruirResumenAsync(periodo, ct);
        var f = r.Flujo;
        return f.BalanceInicial + f.IngresosActual - f.FijosActual - f.NecesariosActual
               - f.DeudasActual - f.AhorrosActual - f.SituacionalesActual;
    }

    private async Task<ResumenPeriodoDto> ConstruirResumenAsync(Periodo periodo, CancellationToken ct)
    {
        var categorias = await db.Categorias.ToListAsync(ct);
        var catById = categorias.ToDictionary(c => c.Id);
        var snapshot = await db.PeriodoCategorias.Where(pc => pc.PeriodoId == periodo.Id).ToListAsync(ct);
        var movs = await db.Movimientos.Where(m => m.PeriodoId == periodo.Id).ToListAsync(ct);

        // Unión del snapshot + categorías activas que se crearon DESPUÉS de tomarlo.
        // El snapshot conserva los montos/membresía congelados del mes (desactivar no borra la
        // línea en curso); además, una categoría activa nueva aparece de inmediato en el mes
        // (con su presupuesto actual), sin tener que recrear el periodo.
        var enSnapshot = snapshot.Select(pc => pc.CategoriaId).ToHashSet();
        List<(Categoria cat, decimal pres)> baseLineas = snapshot.Count > 0
            ? snapshot.Where(pc => catById.ContainsKey(pc.CategoriaId))
                      .Select(pc => (catById[pc.CategoriaId], pc.MontoPresupuestado))
                      .Concat(categorias
                          .Where(c => c.Activo && !enSnapshot.Contains(c.Id))
                          .Select(c => (c, c.Presupuesto)))
                      .ToList()
            : categorias.Where(c => c.Activo).Select(c => (c, c.Presupuesto)).ToList();

        var actualByCat = movs
            .Where(m => m.CategoriaId != null)
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto));
        decimal Actual(Guid catId) => actualByCat.TryGetValue(catId, out var v) ? v : 0m;

        var secciones = new List<SeccionResumenDto>();
        foreach (var tipo in Enum.GetValues<Tipo>())
        {
            if (tipo == Tipo.Situacional) continue; // se maneja aparte (sin catálogo)
            var lineas = baseLineas
                .Where(b => b.cat.Tipo == tipo)
                .OrderBy(b => b.cat.Orden)
                .Select(b =>
                {
                    var actual = Calc.Round2(Actual(b.cat.Id));
                    return new LineaResumenDto(b.cat.Id, b.cat.Nombre, tipo, b.pres, actual,
                        Calc.Round2(b.pres - actual), b.cat.FechaVencimiento, b.cat.Emoji);
                })
                .ToList();

            secciones.Add(new SeccionResumenDto(
                tipo, lineas,
                Calc.Round2(lineas.Sum(l => l.MontoPresupuestado)),
                Calc.Round2(lineas.Sum(l => l.Actual))));
        }

        var situacionales = movs
            .Where(m => m.Tipo == Tipo.Situacional)
            .OrderBy(m => m.Fecha)
            .Select(m => new SituacionalDto(m.Id, m.Fecha, m.Concepto ?? string.Empty, m.Monto))
            .ToList();
        var situacionalesActual = Calc.Round2(situacionales.Sum(s => s.Monto));

        SeccionResumenDto Sec(Tipo t) => secciones.First(s => s.Tipo == t);
        var gastoPres = Calc.TiposGasto.Sum(t => Sec(t).TotalPresupuestado);
        var gastoActual = Calc.TiposGasto.Sum(t => Sec(t).TotalActual);

        var flujo = new FlujoResumenDto(
            periodo.BalanceInicial,
            Sec(Tipo.Ingreso).TotalPresupuestado, Sec(Tipo.Ingreso).TotalActual,
            Sec(Tipo.Fijo).TotalPresupuestado, Sec(Tipo.Fijo).TotalActual,
            Sec(Tipo.Necesario).TotalPresupuestado, Sec(Tipo.Necesario).TotalActual,
            Sec(Tipo.Deuda).TotalPresupuestado, Sec(Tipo.Deuda).TotalActual,
            Sec(Tipo.Ahorro).TotalPresupuestado, Sec(Tipo.Ahorro).TotalActual,
            situacionalesActual,
            Calc.Round2(gastoPres - gastoActual - situacionalesActual));

        // "Dinero disponible" realista: lo que tienes ahora (balance + ingresos recibidos − TODO lo
        // ya pagado) menos lo que AÚN debes pagar de fijos, deudas y ahorros (pendiente = presupuesto
        // − pagado, si es > 0). Los necesarios ya gastados cuentan como pagado; los pendientes no se
        // reservan (son el gasto discrecional que este disponible representa).
        decimal Pendiente(decimal pres, decimal act) => Math.Max(0, pres - act);
        var tengoAhora = flujo.BalanceInicial + flujo.IngresosActual
            - (flujo.FijosActual + flujo.NecesariosActual + flujo.DeudasActual + flujo.AhorrosActual + flujo.SituacionalesActual);
        var porPagar = Pendiente(flujo.FijosPresupuesto, flujo.FijosActual)
            + Pendiente(flujo.DeudasPresupuesto, flujo.DeudasActual)
            + Pendiente(flujo.AhorrosPresupuesto, flujo.AhorrosActual);
        var disponible = Calc.Round2(tengoAhora - porPagar);

        return new ResumenPeriodoDto(periodo.ToDto(), secciones, situacionales, flujo, disponible);
    }
}
