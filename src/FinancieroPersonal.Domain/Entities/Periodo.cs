using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Domain.Entities;

public class Periodo : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Anio { get; set; }

    /// <summary>Mes 1-12.</summary>
    public int Mes { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Moneda { get; set; } = "PEN";
    public decimal BalanceInicial { get; set; }
    public EstadoPeriodo Estado { get; set; } = EstadoPeriodo.Borrador;

    public bool Eliminado { get; set; }
    public DateTime? EliminadoEn { get; set; }
}
