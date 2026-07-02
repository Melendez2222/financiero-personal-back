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
    string? Emoji,
    bool Activo,
    CoberturaIngreso? Cobertura,
    bool Cerrado,
    string? Justificacion);

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
    decimal TotalRestante,
    /// <summary>Interés pagado en deudas Iniciadas este mes (pago completo − capital recuperado).</summary>
    decimal InteresesActual);

public record ResumenPeriodoDto(
    PeriodoDto Periodo,
    IReadOnlyList<SeccionResumenDto> Secciones,
    IReadOnlyList<SituacionalDto> Situacionales,
    FlujoResumenDto Flujo,
    /// <summary>Saldo disponible reservando compromisos pendientes (estimado si no cumplido, real si cumplido).</summary>
    decimal Disponible,
    /// <summary>Saldo actual crudo: balance + ingresos recibidos − gastos pagados. Sin reservar pendientes.</summary>
    decimal SaldoActual,
    decimal MetasPorAportar);

/// <summary>Marcar una categoría como cumplida en un periodo (con justificación opcional).</summary>
public record CrearCierreRequest(Guid CategoriaId, string? Justificacion);

/// <summary>Una categoría de gasto que quedó pendiente (queda &gt; 0) en un mes concreto (cuentas por pagar cross-mes).</summary>
public record PendienteGastoDto(
    Guid PeriodoId,
    int Anio,
    int Mes,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    Guid CategoriaId,
    string Nombre,
    Tipo Tipo,
    string? Emoji,
    CoberturaIngreso? Cobertura,
    decimal MontoPendiente);
