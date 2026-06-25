using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Domain.Entities;

/// <summary>Movimiento concreto (transacción). Alimenta los totales "actual".</summary>
public class Movimiento : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PeriodoId { get; set; }

    /// <summary>Categoría del catálogo. Null en gastos situacionales (concepto libre).</summary>
    public Guid? CategoriaId { get; set; }

    /// <summary>Concepto libre para gastos situacionales (sin categoría).</summary>
    public string? Concepto { get; set; }
    public Tipo Tipo { get; set; }

    /// <summary>Persona/usuario a quien se atribuye el movimiento. Opcional.</summary>
    public Guid? UsuarioId { get; set; }
    public DateOnly Fecha { get; set; }

    /// <summary>Siempre positivo; el signo lo deriva la UI según el tipo.</summary>
    public decimal Monto { get; set; }

    /// <summary>
    /// Solo abonos a deuda: porción de este abono que reduce el capital de la deuda.
    /// Null = el Monto completo reduce el capital (deudas sin interés y back-compat).
    /// </summary>
    public decimal? MontoCapital { get; set; }

    /// <summary>
    /// Solo abonos a deuda: true = cuota regular (cuenta para "cuotas pagadas");
    /// false = abono extra a capital. Default true (back-compat y pagos normales).
    /// </summary>
    public bool EsCuota { get; set; } = true;
    public string Nota { get; set; } = string.Empty;

    public bool Eliminado { get; set; }
    public DateTime? EliminadoEn { get; set; }
}
