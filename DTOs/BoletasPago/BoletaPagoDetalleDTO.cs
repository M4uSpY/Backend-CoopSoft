using System;

namespace BackendCoopSoft.DTOs.BoletasPago;

public class BoletaPagoDetalleDTO
{
    // Encabezado
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Lugar { get; set; } = "La Paz";
    public int Gestion { get; set; }
    public int Mes { get; set; }
    public string MesNombre { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public int DiasTrabajados { get; set; }

    // Ingresos
    public decimal SueldoBasico { get; set; }
    public decimal SbPorDiasTrabajados { get; set; }
    public decimal BonoAntiguedad { get; set; }
    public decimal OtrosPagos { get; set; }
    public decimal OIAporteInstitucional { get; set; } // AP_COOP_334
    public decimal TotalGanado { get; set; }

    // Descuentos
    public decimal OtrosDesc { get; set; }                   // Otros Descuentos
    public decimal Iva { get; set; }                   // RC_IVA_13
    public decimal AporteGestora { get; set; }         // GESTORA_1221
    public decimal AporteProvivienda { get; set; }     // si no manejas, 0
    public decimal AporteSolidario { get; set; }       // AP_SOL_05
    public decimal OtrosDescuentos { get; set; }       // OTROS_DESC_668 (+ OTROS_DESC)
    public decimal TotalDescuentos { get; set; }

    public decimal LiquidoPagable { get; set; }
}
