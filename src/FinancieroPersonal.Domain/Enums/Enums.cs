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

/// <summary>Bolsa de ingreso que cubre un gasto: la quincena (mitad de mes) o el sueldo de fin de mes.</summary>
public enum CoberturaIngreso
{
    Quincena,
    FinDeMes,
}

/// <summary>
/// Estado del ciclo de vida de una deuda, controlado manualmente por el usuario.
/// Solo las deudas <see cref="Iniciada"/> entran en los cálculos del mes.
/// <para>
/// <see cref="Iniciada"/> va primero a propósito: es el valor por defecto del CLR y coincide con el
/// default de la columna (backfill de filas existentes). Así una deuda nueva con cualquier otro
/// estado (p. ej. <see cref="Pendiente"/>) se inserta tal cual, sin que EF aplique el default de BD.
/// </para>
/// </summary>
public enum EstadoDeuda
{
    /// <summary>Activada e iniciada: se está pagando y cuenta en el panel del mes.</summary>
    Iniciada,
    /// <summary>Creada pero aún no iniciada (p. ej. deuda futura, o iniciada por error y revertida).</summary>
    Pendiente,
    /// <summary>Pausada temporalmente: no cuenta en los cálculos.</summary>
    Suspendida,
    /// <summary>Saldada: ya no cuenta en los cálculos.</summary>
    Saldada,
}
