using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

public record CategoriaDto(
    Guid Id,
    string Nombre,
    Tipo Tipo,
    decimal Presupuesto,
    string? Emoji,
    string? FechaVencimiento,
    int? CuotasRestantes,
    decimal? MontoTotal,
    decimal? CapitalPorCuota,
    TipoDeuda? TipoDeuda,
    bool Activo,
    int Orden);

public record CrearCategoriaRequest(
    string Nombre,
    Tipo Tipo,
    decimal Presupuesto,
    string? Emoji,
    string? FechaVencimiento,
    int? CuotasRestantes,
    decimal? MontoTotal,
    decimal? CapitalPorCuota,
    TipoDeuda? TipoDeuda,
    bool? Activo);

public record ActualizarCategoriaRequest(
    string? Nombre,
    decimal? Presupuesto,
    string? Emoji,
    string? FechaVencimiento,
    int? CuotasRestantes,
    decimal? MontoTotal,
    decimal? CapitalPorCuota,
    TipoDeuda? TipoDeuda,
    bool? Activo);

public record SetActivoRequest(bool Activo);
