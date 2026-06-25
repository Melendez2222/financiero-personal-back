namespace FinancieroPersonal.Domain.Entities;

/// <summary>
/// Snapshot de las categorías incluidas en un periodo (con su presupuesto al momento de iniciarlo).
/// Garantiza que desactivar una categoría luego no la quite de un mes ya iniciado.
/// </summary>
public class PeriodoCategoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal MontoPresupuestado { get; set; }
}
