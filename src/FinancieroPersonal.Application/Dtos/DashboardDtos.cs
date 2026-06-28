using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

public record KpiValorDto(decimal Actual, decimal DeltaPct);

public record KpisDto(
    KpiValorDto Ingresos,
    KpiValorDto Fijos,
    KpiValorDto Necesarios,
    KpiValorDto Deudas,
    KpiValorDto Ahorros);

public record FlujoMesDto(string Mes, decimal Ingresos, decimal Gastos);

public record DesgloseDto(string Categoria, Tipo Tipo, decimal Monto, decimal Pct);

public record MetaProgresoDto(Guid Id, string Nombre, decimal Pct, decimal Actual, decimal? Objetivo);

public record DashboardDto(
    KpisDto Kpis,
    IReadOnlyList<FlujoMesDto> FlujoMeses,
    IReadOnlyList<DesgloseDto> Desglose,
    decimal Disponible,
    IReadOnlyList<MetaProgresoDto> Metas);
