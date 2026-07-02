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
        await AgregarSnapshotAsync(periodo, ct);
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

        await AgregarSnapshotAsync(p, ct);
        p.Estado = EstadoPeriodo.Iniciado;
        await db.SaveChangesAsync(ct);
        return p.ToDto();
    }

    public async Task<ResumenPeriodoDto> ResumenAsync(Guid id, Guid? usuarioId, CancellationToken ct)
    {
        var p = await db.Periodos.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw AppException.NotFound("Periodo no encontrado.");
        return await ConstruirResumenAsync(p, usuarioId, ct);
    }

    /// <summary>
    /// Cuentas por pagar cross-mes: todas las líneas de gasto (Fijo/Necesario/Ahorro) con `Queda > 0`
    /// de TODOS los periodos (pasados + actual), cada una con el mes al que pertenece. Reusa el mismo
    /// cálculo del resumen para que los montos cuadren con el Panel del mes.
    /// </summary>
    public async Task<List<PendienteGastoDto>> PendientesGastosAsync(CancellationToken ct)
    {
        var periodos = await db.Periodos
            .OrderBy(p => p.Anio).ThenBy(p => p.Mes)
            .ToListAsync(ct);

        var tipos = new HashSet<Tipo> { Tipo.Fijo, Tipo.Necesario, Tipo.Ahorro };
        var pendientes = new List<PendienteGastoDto>();
        foreach (var p in periodos)
        {
            var resumen = await ConstruirResumenAsync(p, null, ct);
            foreach (var sec in resumen.Secciones.Where(s => tipos.Contains(s.Tipo)))
                foreach (var l in sec.Lineas.Where(l => l.Queda > 0.005m))
                    pendientes.Add(new PendienteGastoDto(
                        p.Id, p.Anio, p.Mes, p.FechaInicio, p.FechaFin,
                        l.CategoriaId, l.Nombre, l.Tipo, l.Emoji, l.Cobertura, Calc.Round2(l.Queda)));
        }
        return pendientes;
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

    /// <summary>
    /// Crea el snapshot de categorías activas Y vigentes para un periodo (si aún no existe).
    /// Una categoría con vigencia (desde/hasta) solo entra si su rango cubre el mes del periodo.
    /// </summary>
    private async Task AgregarSnapshotAsync(Periodo periodo, CancellationToken ct)
    {
        var yaTiene = await db.PeriodoCategorias.AnyAsync(pc => pc.PeriodoId == periodo.Id, ct);
        if (yaTiene) return;

        var inicioMes = periodo.FechaInicio;
        var activas = await db.Categorias.Where(c => c.Activo).ToListAsync(ct);
        foreach (var c in activas.Where(c => VigenteEn(c, inicioMes)))
        {
            db.PeriodoCategorias.Add(new PeriodoCategoria
            {
                PeriodoId = periodo.Id,
                CategoriaId = c.Id,
                MontoPresupuestado = c.Presupuesto,
            });
        }
    }

    /// <summary>True si la vigencia de la categoría cubre el primer día del mes dado (null = sin límite).</summary>
    private static bool VigenteEn(Categoria c, DateOnly inicioMes) =>
        (c.VigenciaDesde is null || inicioMes >= c.VigenciaDesde)
        && (c.VigenciaHasta is null || inicioMes <= c.VigenciaHasta);

    private async Task<decimal> BalanceFinalAsync(Periodo periodo, CancellationToken ct)
    {
        // La herencia de balance es del hogar (global), nunca por persona.
        var r = await ConstruirResumenAsync(periodo, null, ct);
        var f = r.Flujo;
        return f.BalanceInicial + f.IngresosActual - f.FijosActual - f.NecesariosActual
               - f.DeudasActual - f.AhorrosActual - f.SituacionalesActual;
    }

    /// <summary>
    /// Construye el resumen del periodo. Si <paramref name="usuarioId"/> no es null, filtra a esa
    /// persona: solo movimientos atribuidos a ella (actual) y solo categorías cuya persona por
    /// defecto es ella (presupuesto). Null = vista global del hogar.
    /// </summary>
    private async Task<ResumenPeriodoDto> ConstruirResumenAsync(Periodo periodo, Guid? usuarioId, CancellationToken ct)
    {
        var categorias = await db.Categorias.ToListAsync(ct);
        var catById = categorias.ToDictionary(c => c.Id);
        var snapshot = await db.PeriodoCategorias.Where(pc => pc.PeriodoId == periodo.Id).ToListAsync(ct);
        var movs = await db.Movimientos
            .Where(m => m.PeriodoId == periodo.Id)
            .Where(m => usuarioId == null || m.UsuarioId == usuarioId)
            .ToListAsync(ct);
        // Gastos financiados desde ahorro no afectan el flujo del mes (disponible / actuales).
        var movsFlujo = movs.Where(m => m.MetaId == null).ToList();

        // Unión del snapshot + categorías activas que se crearon DESPUÉS de tomarlo.
        // El snapshot conserva los montos/membresía congelados del mes (desactivar no borra la
        // línea en curso); además, una categoría activa nueva aparece de inmediato en el mes
        // (con su presupuesto actual), sin tener que recrear el periodo.
        // La vigencia solo se aplica a las categorías que se añaden FUERA del snapshot (el snapshot
        // es la verdad congelada del mes: si está ahí, pertenece al mes aunque su vigencia cambie luego).
        var inicioMes = periodo.FechaInicio;
        var enSnapshot = snapshot.Select(pc => pc.CategoriaId).ToHashSet();
        List<(Categoria cat, decimal pres)> baseLineas = snapshot.Count > 0
            ? snapshot.Where(pc => catById.ContainsKey(pc.CategoriaId))
                      .Select(pc => (catById[pc.CategoriaId], pc.MontoPresupuestado))
                      .Concat(categorias
                          .Where(c => c.Activo && !enSnapshot.Contains(c.Id) && VigenteEn(c, inicioMes))
                          .Select(c => (c, c.Presupuesto)))
                      .ToList()
            : categorias.Where(c => c.Activo && VigenteEn(c, inicioMes)).Select(c => (c, c.Presupuesto)).ToList();

        // Vista por persona: el presupuesto también se acota a las categorías de esa persona.
        if (usuarioId != null)
            baseLineas = baseLineas.Where(b => b.cat.UsuarioId == usuarioId).ToList();

        var movsByCat = movsFlujo
            .Where(m => m.CategoriaId != null)
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        // Pago completo (capital + interés) — lo que salió del bolsillo.
        decimal ActualMonto(Guid catId) => movsByCat.TryGetValue(catId, out var l) ? l.Sum(m => m.Monto) : 0m;
        decimal ActualMontoCobertura(Guid catId, CoberturaIngreso? cobertura) =>
            movsByCat.TryGetValue(catId, out var l)
                ? l.Where(m => m.Cobertura == cobertura).Sum(m => m.Monto)
                : 0m;
        // Solo capital recuperado (MontoCapital ?? Monto) — lo que realmente baja la deuda.
        decimal ActualCapital(Guid catId) => movsByCat.TryGetValue(catId, out var l) ? l.Sum(m => m.MontoCapital ?? m.Monto) : 0m;

        static bool CatDividida(Categoria c) => c.MontoQuincena is not null && c.MontoFinDeMes is not null;

        var secciones = new List<SeccionResumenDto>();
        foreach (var tipo in Enum.GetValues<Tipo>())
        {
            if (tipo == Tipo.Situacional) continue; // se maneja aparte (sin catálogo)
            var esDeuda = tipo == Tipo.Deuda;
            var lineas = new List<LineaResumenDto>();
            foreach (var b in baseLineas
                         .Where(b => b.cat.Tipo == tipo
                             && (!esDeuda || b.cat.EstadoDeuda == EstadoDeuda.Iniciada))
                         .OrderBy(b => b.cat.Orden))
            {
                if (!esDeuda && CatDividida(b.cat))
                {
                    var mq = b.cat.MontoQuincena!.Value;
                    var mf = b.cat.MontoFinDeMes!.Value;
                    var actualQ = Calc.Round2(ActualMontoCobertura(b.cat.Id, CoberturaIngreso.Quincena));
                    var actualF = Calc.Round2(ActualMontoCobertura(b.cat.Id, CoberturaIngreso.FinDeMes));
                    lineas.Add(new LineaResumenDto(
                        b.cat.Id, b.cat.Nombre, tipo, mq, actualQ, Calc.Round2(mq - actualQ),
                        b.cat.FechaVencimiento, b.cat.Emoji, b.cat.Activo, CoberturaIngreso.Quincena));
                    lineas.Add(new LineaResumenDto(
                        b.cat.Id, b.cat.Nombre, tipo, mf, actualF, Calc.Round2(mf - actualF),
                        b.cat.FechaVencimiento, b.cat.Emoji, b.cat.Activo, CoberturaIngreso.FinDeMes));
                }
                else
                {
                    var actual = Calc.Round2(esDeuda ? ActualCapital(b.cat.Id) : ActualMonto(b.cat.Id));
                    var pres = esDeuda ? (b.cat.CapitalPorCuota ?? b.pres) : b.pres;
                    lineas.Add(new LineaResumenDto(
                        b.cat.Id, b.cat.Nombre, tipo, pres, actual, Calc.Round2(pres - actual),
                        b.cat.FechaVencimiento, b.cat.Emoji, b.cat.Activo, b.cat.Cobertura));
                }
            }

            // Ingresos extra (sin categoría del catálogo): una línea agregada para que cuenten en el total.
            if (tipo == Tipo.Ingreso)
            {
                var extrasActual = Calc.Round2(
                    movsFlujo.Where(m => m.Tipo == Tipo.Ingreso && m.CategoriaId == null).Sum(m => m.Monto));
                if (extrasActual > 0)
                {
                    lineas.Add(new LineaResumenDto(
                        Guid.Empty, "Otros ingresos", tipo, 0m, extrasActual, Calc.Round2(-extrasActual),
                        null, null, true, null));
                }
            }

            secciones.Add(new SeccionResumenDto(
                tipo, lineas,
                Calc.Round2(lineas.Sum(l => l.MontoPresupuestado)),
                Calc.Round2(lineas.Sum(l => l.Actual))));
        }

        // Interés pagado este mes en deudas Iniciadas (pago completo − capital). Sale del bolsillo
        // (resta del saldo) pero se contabiliza aparte de la recuperación de capital.
        var interesesActual = Calc.Round2(
            baseLineas
                .Where(b => b.cat.Tipo == Tipo.Deuda && b.cat.EstadoDeuda == EstadoDeuda.Iniciada)
                .Sum(b => ActualMonto(b.cat.Id) - ActualCapital(b.cat.Id)));

        var situacionales = movsFlujo
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
            Calc.Round2(gastoPres - gastoActual - situacionalesActual),
            interesesActual);

        // "Saldo disponible" = SALDO ACTUAL real: lo que tienes ahora = balance inicial + ingresos ya
        // recibidos − TODO lo ya pagado (fijos/necesarios/deudas/ahorros/situacionales actuales). NO
        // reserva compromisos pendientes; la proyección de fin de mes la calcula el frontend a partir
        // del flujo + metasPorAportar.
        // DeudasActual ahora es solo capital; el interés pagado también salió del bolsillo, así que se
        // resta aparte → el saldo queda idéntico a sumar la cuota completa.
        var tengoAhora = flujo.BalanceInicial + flujo.IngresosActual
            - (flujo.FijosActual + flujo.NecesariosActual + flujo.DeudasActual + flujo.AhorrosActual
               + flujo.SituacionalesActual + flujo.InteresesActual);

        // Metas de ahorro activas: aporte mensual que aún no se ha cubierto ESTE mes (lo usa el front
        // para la proyección secundaria). Solo en vista global (las metas son del hogar).
        // (aportes con fecha dentro del periodo). Solo en vista global (las metas son del hogar).
        decimal metasPorAportar = 0m;
        if (usuarioId == null)
        {
            var aportesMes = (await db.Aportes
                    .Where(a => a.Fecha >= periodo.FechaInicio && a.Fecha <= periodo.FechaFin)
                    .ToListAsync(ct))
                .GroupBy(a => a.MetaId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Monto));
            var metasActivas = await db.Metas.Where(m => m.Activo).ToListAsync(ct);
            metasPorAportar = Calc.Round2(
                metasActivas.Sum(m => Math.Max(0m, m.AporteMensual - aportesMes.GetValueOrDefault(m.Id))));
        }

        var disponible = Calc.Round2(tengoAhora);

        return new ResumenPeriodoDto(periodo.ToDto(), secciones, situacionales, flujo, disponible, metasPorAportar);
    }
}
