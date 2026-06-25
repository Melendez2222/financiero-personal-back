using FinancieroPersonal.Domain.Enums;

namespace FinancieroPersonal.Application.Dtos;

public record MovimientoDto(
    Guid Id,
    Guid PeriodoId,
    Guid? CategoriaId,
    string? Concepto,
    Tipo Tipo,
    DateOnly Fecha,
    decimal Monto,
    string Nota,
    Guid? UsuarioId);

public record CrearMovimientoRequest(
    Guid PeriodoId,
    Guid? CategoriaId,
    string? Concepto,
    Tipo Tipo,
    DateOnly Fecha,
    decimal Monto,
    string? Nota,
    Guid? UsuarioId);

public record ActualizarMovimientoRequest(
    Guid? CategoriaId,
    string? Concepto,
    Tipo? Tipo,
    DateOnly? Fecha,
    decimal? Monto,
    string? Nota,
    Guid? UsuarioId);
