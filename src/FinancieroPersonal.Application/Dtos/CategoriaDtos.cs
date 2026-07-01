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
    Guid? UsuarioId,
    bool Activo,
    int Orden,
    CoberturaIngreso? Cobertura,
    decimal? MontoQuincena,
    decimal? MontoFinDeMes,
    DateOnly? VigenciaDesde,
    DateOnly? VigenciaHasta,
    EstadoDeuda EstadoDeuda);

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
    Guid? UsuarioId,
    bool? Activo,
    CoberturaIngreso? Cobertura,
    decimal? MontoQuincena,
    decimal? MontoFinDeMes,
    DateOnly? VigenciaDesde,
    DateOnly? VigenciaHasta,
    EstadoDeuda? EstadoDeuda);

public record ActualizarCategoriaRequest(
    string? Nombre,
    decimal? Presupuesto,
    string? Emoji,
    string? FechaVencimiento,
    int? CuotasRestantes,
    decimal? MontoTotal,
    decimal? CapitalPorCuota,
    TipoDeuda? TipoDeuda,
    Guid? UsuarioId,
    bool? Activo,
    CoberturaIngreso? Cobertura,
    decimal? MontoQuincena,
    decimal? MontoFinDeMes,
    DateOnly? VigenciaDesde,
    DateOnly? VigenciaHasta,
    EstadoDeuda? EstadoDeuda);

public record SetActivoRequest(bool Activo);

public record SetEstadoDeudaRequest(EstadoDeuda EstadoDeuda);

public record SetCoberturaRequest(CoberturaIngreso? Cobertura);
