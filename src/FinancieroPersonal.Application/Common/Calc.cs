using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Common;

public static class Calc
{
    /// <summary>Tipos de gasto presupuestables (tienen línea en el catálogo).</summary>
    public static readonly Tipo[] TiposGasto = [Tipo.Fijo, Tipo.Necesario, Tipo.Deuda];

    /// <summary>Tipos de gasto para sumar el gasto real (incluye situacionales, sin presupuesto).</summary>
    public static readonly Tipo[] TiposGastoActual = [Tipo.Fijo, Tipo.Necesario, Tipo.Deuda, Tipo.Situacional];

    public static readonly string[] MesesAbbr =
        ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

    public static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static bool EsGasto(Tipo tipo) => Array.IndexOf(TiposGasto, tipo) >= 0;
}
