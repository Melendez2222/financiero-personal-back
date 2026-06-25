namespace FinancieroPersonal.Domain.Entities;

/// <summary>Aporte concreto a una meta de ahorro (historial, con descripción opcional).</summary>
public class AporteMeta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MetaId { get; set; }
    public decimal Monto { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Descripcion { get; set; }
}
