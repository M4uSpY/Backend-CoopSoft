using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class ConceptoPlanillaDTO
{
    public int IdConcepto { get; set; }
    public string Nombre { get; set; } = string.Empty; // p.ej., "Aporte Solidario", "Otros Descuentos"
    public bool EsIngreso { get; set; } // true=Haber, false=Descuento
    public decimal Importe { get; set; }
}
