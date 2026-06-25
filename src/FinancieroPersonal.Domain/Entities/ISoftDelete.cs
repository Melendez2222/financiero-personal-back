namespace FinancieroPersonal.Domain.Entities;

/// <summary>
/// Marca de borrado lógico. Las entidades que la implementan nunca se eliminan físicamente:
/// se marca Eliminado=true (con fecha) y un filtro global de EF las oculta de todas las consultas,
/// pero permanecen en la BD para auditoría.
/// </summary>
public interface ISoftDelete
{
    bool Eliminado { get; set; }
    DateTime? EliminadoEn { get; set; }
}
