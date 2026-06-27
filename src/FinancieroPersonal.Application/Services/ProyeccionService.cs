using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Application.Common;
using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinancieroPersonal.Application.Services;

public class ProyeccionService(IAppDbContext db, PeriodoService periodos)
{
    /// <summary>
    /// Guía mes a mes: proyecta ingresos/gastos fijos+necesarios constantes, deudas que se
    /// saldan al agotar sus cuotas y metas que se completan; emite hitos y acumula el saldo.
    /// </summary>
    public async Task<GuiaDto> GetAsync(int meses, CancellationToken ct)
    {
        if (meses < 1) meses = 12;
        if (meses > 60) meses = 60;

        var activas = await db.Categorias.Where(c => c.Activo).ToListAsync(ct);
        decimal SumaTipo(Tipo t) => Calc.Round2(activas.Where(c => c.Tipo == t).Sum(c => c.Presupuesto));
        var ingresos = SumaTipo(Tipo.Ingreso);
        var fijos = SumaTipo(Tipo.Fijo);
        var necesarios = SumaTipo(Tipo.Necesario);

        var pagosDeuda = (await db.Movimientos
                .Where(m => m.Tipo == Tipo.Deuda && m.CategoriaId != null)
                .ToListAsync(ct))
            .GroupBy(m => m.CategoriaId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto));

        var deudas = activas
            .Where(c => c.Tipo == Tipo.Deuda)
            .Select(c =>
            {
                // Si la deuda tiene monto total, las cuotas que faltan salen del saldo restante.
                int? cuotas = c.CuotasRestantes;
                if (c.MontoTotal is { } total && c.Presupuesto > 0)
                {
                    var saldo = Math.Max(0, total - pagosDeuda.GetValueOrDefault(c.Id));
                    cuotas = (int)Math.Ceiling(saldo / c.Presupuesto);
                }
                return new { c.Id, c.Nombre, Cuota = c.Presupuesto, CuotasRestantes = cuotas };
            })
            .ToList();
        var restantes = deudas.ToDictionary(d => d.Id, d => d.CuotasRestantes);
        var deudaTerminada = deudas.ToDictionary(d => d.Id, _ => false);

        var metas = (await db.Metas.ToListAsync(ct))
            .Where(m => m.Activo && m.AporteMensual > 0 && m.MontoAcumulado < m.MontoObjetivo)
            .Select(m => new { m.Id, m.Nombre, m.AporteMensual, m.MontoObjetivo, Acumulado = m.MontoAcumulado })
            .ToList();
        var acumuladoMeta = metas.ToDictionary(m => m.Id, m => m.Acumulado);
        var metaCompleta = metas.ToDictionary(m => m.Id, _ => false);

        var ordenados = (await db.Periodos.ToListAsync(ct))
            .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes).ToList();
        var ultimo = ordenados.FirstOrDefault();

        decimal saldo;
        int desdeAnio;
        int desdeMes;
        if (ultimo is not null)
        {
            var f = (await periodos.ResumenAsync(ultimo.Id, null, ct)).Flujo;
            saldo = Calc.Round2(
                f.BalanceInicial + f.IngresosActual - f.FijosActual - f.NecesariosActual
                - f.DeudasActual - f.AhorrosActual - f.SituacionalesActual);
            desdeAnio = ultimo.Mes == 12 ? ultimo.Anio + 1 : ultimo.Anio;
            desdeMes = ultimo.Mes == 12 ? 1 : ultimo.Mes + 1;
        }
        else
        {
            var hoy = DateTime.UtcNow;
            saldo = 0m;
            desdeAnio = hoy.Year;
            desdeMes = hoy.Month;
        }

        var saldoInicial = saldo;
        var resultado = new List<GuiaMesDto>();

        for (var i = 0; i < meses; i++)
        {
            var idx = desdeMes - 1 + i;
            var anio = desdeAnio + idx / 12;
            var mes = idx % 12 + 1;
            var hitos = new List<string>();

            decimal deudasMes = 0;
            foreach (var d in deudas)
            {
                if (deudaTerminada[d.Id]) continue;
                var r = restantes[d.Id];
                if (r is null)
                {
                    deudasMes += d.Cuota; // deuda sin fecha de término
                    continue;
                }
                if (r.Value > 0)
                {
                    deudasMes += d.Cuota;
                    restantes[d.Id] = r.Value - 1;
                    if (restantes[d.Id] == 0)
                    {
                        deudaTerminada[d.Id] = true;
                        hitos.Add($"Terminas de pagar {d.Nombre}");
                    }
                }
            }

            decimal ahorroMes = 0;
            foreach (var m in metas)
            {
                if (metaCompleta[m.Id]) continue;
                ahorroMes += m.AporteMensual;
                acumuladoMeta[m.Id] += m.AporteMensual;
                if (acumuladoMeta[m.Id] >= m.MontoObjetivo)
                {
                    metaCompleta[m.Id] = true;
                    hitos.Add($"Completas la meta {m.Nombre}");
                }
            }

            var neto = Calc.Round2(ingresos - fijos - necesarios - deudasMes - ahorroMes);
            saldo = Calc.Round2(saldo + neto);

            resultado.Add(new GuiaMesDto(
                anio, mes, $"{Calc.MesesAbbr[mes - 1]} {anio}",
                ingresos, fijos, necesarios,
                Calc.Round2(deudasMes), Calc.Round2(ahorroMes), neto, saldo, hitos));
        }

        return new GuiaDto(desdeAnio, desdeMes, saldoInicial, resultado);
    }
}
