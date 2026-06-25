using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Domain.Entities;

public class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public Tipo Tipo { get; set; }

    /// <summary>Presupuesto/cuota mensual (fuente única, global).</summary>
    public decimal Presupuesto { get; set; }
    public string? Emoji { get; set; }

    /// <summary>Día de vencimiento dentro del mes, p.ej. "15".</summary>
    public string? FechaVencimiento { get; set; }

    /// <summary>Solo deudas: nº de cuotas pendientes. Null = sin fecha de término.</summary>
    public int? CuotasRestantes { get; set; }

    /// <summary>Solo deudas: monto total de la deuda. El saldo restante = total − pagos.</summary>
    public decimal? MontoTotal { get; set; }

    /// <summary>
    /// Solo deudas con interés: monto fijo de cada cuota regular que reduce el capital
    /// (el resto de la cuota es interés). Null = deuda sin interés (el abono completo baja el saldo).
    /// </summary>
    public decimal? CapitalPorCuota { get; set; }

    /// <summary>Activador: si está activa se aplica a los periodos nuevos.</summary>
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}
