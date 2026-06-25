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
    decimal TotalPagado,
    decimal? SaldoRestante,
    decimal? Pct,
    bool Activo);
