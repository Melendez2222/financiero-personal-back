using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Domain.Entities;

public class MetaAhorro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public decimal MontoObjetivo { get; set; }
    public decimal AporteMensual { get; set; }
    public decimal MontoAcumulado { get; set; }

    /// <summary>Aportado en el mes activo (real).</summary>
    public decimal AporteMes { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public EstadoMeta Estado { get; set; } = EstadoMeta.NoIniciado;
    public bool Activo { get; set; }
}
