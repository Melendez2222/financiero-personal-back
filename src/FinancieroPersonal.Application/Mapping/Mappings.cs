using FinancieroPersonal.Application.Dtos;
using FinancieroPersonal.Domain.Entities;

namespace FinancieroPersonal.Application.Mapping;

public static class Mappings
{
    public static UsuarioDto ToDto(this Usuario u) => new(u.Id, u.Email, u.Nombre, u.Apellidos);

    public static CategoriaDto ToDto(this Categoria c) =>
        new(c.Id, c.Nombre, c.Tipo, c.Presupuesto, c.Emoji, c.FechaVencimiento, c.CuotasRestantes, c.MontoTotal, c.CapitalPorCuota, c.TipoDeuda, c.UsuarioId, c.Activo, c.Orden, c.Cobertura, c.VigenciaDesde, c.VigenciaHasta, c.EstadoDeuda);

    public static PeriodoDto ToDto(this Periodo p) =>
        new(p.Id, p.Anio, p.Mes, p.FechaInicio, p.FechaFin, p.Moneda, p.BalanceInicial, p.Estado);

    public static MovimientoDto ToDto(this Movimiento m) =>
        new(m.Id, m.PeriodoId, m.CategoriaId, m.Concepto, m.Tipo, m.Fecha, m.Monto, m.MontoCapital, m.EsCuota, m.Nota, m.UsuarioId);

    public static MetaDto ToDto(this MetaAhorro m) =>
        new(m.Id, m.Nombre, m.Emoji, m.MontoObjetivo, m.AporteMensual, m.MontoAcumulado, m.AporteMes,
            m.FechaLimite, m.Estado, m.Activo);

    public static ConfiguracionDto ToDto(this Configuracion c) =>
        new(c.Moneda, c.Simbolo, c.Locale, c.Decimales);
}
