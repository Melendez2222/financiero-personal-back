namespace FinancieroPersonal.Application.Dtos;

/// <summary>Un mes proyectado en la guía, con su desglose y los hitos que ocurren ese mes.</summary>
public record GuiaMesDto(
    int Anio,
    int Mes,
    string Etiqueta,
    decimal Ingresos,
    decimal Fijos,
    decimal Necesarios,
    decimal Deudas,
    decimal Ahorro,
    decimal Neto,
    decimal SaldoAcumulado,
    IReadOnlyList<string> Hitos);

public record GuiaDto(
    int DesdeAnio,
    int DesdeMes,
    decimal SaldoInicial,
    IReadOnlyList<GuiaMesDto> Meses);
