using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

public record LineaResumenDto(
    Guid CategoriaId,
    string Nombre,
    Tipo Tipo,
    decimal MontoPresupuestado,
    decimal Actual,
    decimal Queda,
    string? FechaVencimiento,
    string? Emoji);

public record SeccionResumenDto(
    Tipo Tipo,
    IReadOnlyList<LineaResumenDto> Lineas,
    decimal TotalPresupuestado,
    decimal TotalActual);

/// <summary>Gasto situacional (imprevisto sin categoría) del periodo.</summary>
public record SituacionalDto(Guid Id, DateOnly Fecha, string Concepto, decimal Monto);

public record FlujoResumenDto(
    decimal BalanceInicial,
    decimal IngresosPresupuesto,
    decimal IngresosActual,
    decimal FijosPresupuesto,
    decimal FijosActual,
    decimal NecesariosPresupuesto,
    decimal NecesariosActual,
    decimal DeudasPresupuesto,
    decimal DeudasActual,
    decimal AhorrosPresupuesto,
    decimal AhorrosActual,
    decimal SituacionalesActual,
    decimal TotalRestante);

public record ResumenPeriodoDto(
    PeriodoDto Periodo,
    IReadOnlyList<SeccionResumenDto> Secciones,
    IReadOnlyList<SituacionalDto> Situacionales,
    FlujoResumenDto Flujo,
    decimal Disponible);
