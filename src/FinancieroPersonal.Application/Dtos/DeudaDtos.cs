using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

/// <summary>Deuda con su saldo calculado (monto total − pagos registrados).</summary>
public record DeudaDto(
    Guid Id,
    string Nombre,
    string? Emoji,
    string? FechaVencimiento,
    decimal CuotaMensual,
    int? CuotasRestantes,
    decimal? MontoTotal,
    decimal? CapitalPorCuota,
    TipoDeuda? TipoDeuda,
    decimal TotalPagado,
    decimal TotalInteres,
    int CuotasPagadas,
    decimal? SaldoRestante,
    decimal? Pct,
    bool Activo);
