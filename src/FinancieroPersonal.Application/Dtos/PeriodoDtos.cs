using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

public record PeriodoDto(
    Guid Id,
    int Anio,
    int Mes,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string Moneda,
    decimal BalanceInicial,
    EstadoPeriodo Estado);

public record CrearPeriodoRequest(
    int Anio,
    int Mes,
    decimal? BalanceInicial,
    string? Moneda,
    bool? HeredarBalance);

public record ActualizarPeriodoRequest(decimal? BalanceInicial, string? Moneda, EstadoPeriodo? Estado);
