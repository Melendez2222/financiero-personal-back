namespace FinancieroPersonal.Domain.Entities;

/// <summary>
/// "Cumplido": marca que una categoría (Fijo/Necesario/Ahorro) ya se dio por cerrada en un mes,
/// aunque lo pagado/ahorrado sea menor a lo estimado. Su presencia saca la línea de "pendientes" y
/// hace que el saldo disponible reserve solo lo real (no el estimado). Opcional: justificación.
/// </summary>
public class CierreCategoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Justificacion { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
