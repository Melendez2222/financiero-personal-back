namespace FinancieroPersonal.Domain.Entities;

/// <summary>Configuración global (fila única, Id = 1).</summary>
public class Configuracion
{
    public int Id { get; set; } = 1;
    public string Moneda { get; set; } = "PEN";
    public string Simbolo { get; set; } = "S/";
    public string Locale { get; set; } = "es-PE";
    public int Decimales { get; set; } = 2;
}
