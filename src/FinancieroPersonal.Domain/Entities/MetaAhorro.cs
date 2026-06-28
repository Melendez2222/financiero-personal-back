using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Domain.Entities;

public class MetaAhorro : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;

    /// <summary>Monto objetivo de la meta. Null = ahorro abierto (sin meta fija).</summary>
    public decimal? MontoObjetivo { get; set; }
    public decimal AporteMensual { get; set; }
    public decimal MontoAcumulado { get; set; }

    /// <summary>Aportado en el mes activo (real).</summary>
    public decimal AporteMes { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public EstadoMeta Estado { get; set; } = EstadoMeta.NoIniciado;
    public bool Activo { get; set; }

    public bool Eliminado { get; set; }
    public DateTime? EliminadoEn { get; set; }
}
