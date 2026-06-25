namespace FinancieroPersonal.Domain.Enums;

public enum Tipo
{
    Ingreso,
    Fijo,
    Necesario,
    Deuda,
    Ahorro,
    Situacional,
}

public enum EstadoMeta
{
    NoIniciado,
    Pendiente,
    Iniciado,
    Suspendido,
    Finalizado,
}

public enum EstadoPeriodo
{
    Borrador,
    Iniciado,
    Cerrado,
}

public enum TipoDeuda
{
    Prestamo,
    LineaCredito,
}
