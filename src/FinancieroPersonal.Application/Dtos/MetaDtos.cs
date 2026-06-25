using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

public record MetaDto(
    Guid Id,
    string Nombre,
    string Emoji,
    decimal MontoObjetivo,
    decimal AporteMensual,
    decimal MontoAcumulado,
    decimal AporteMes,
    DateOnly? FechaLimite,
    EstadoMeta Estado,
    bool Activo);

public record CrearMetaRequest(
    string Nombre,
    string Emoji,
    decimal MontoObjetivo,
    decimal AporteMensual,
    DateOnly? FechaLimite,
    EstadoMeta? Estado);

public record ActualizarMetaRequest(
    string? Nombre,
    string? Emoji,
    decimal? MontoObjetivo,
    decimal? AporteMensual,
    DateOnly? FechaLimite,
    EstadoMeta? Estado,
    bool? Activo);

public record AporteMetaRequest(decimal Monto, DateOnly Fecha, string? Descripcion, Guid? PeriodoId);

public record AporteDto(Guid Id, decimal Monto, DateOnly Fecha, string? Descripcion);
