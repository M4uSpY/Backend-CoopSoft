using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class PlanillaAportesFilaDTO
{
    public int Id { get; set; }

    public string CarnetIdentidad { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string Ocupacion { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }

    public int DiasPagados { get; set; }

    // Total ganado tomado de la planilla de sueldos
    public decimal TotalGanado { get; set; }

    // APORTES PATRONALES
    public decimal Cps10 { get; set; }
    public decimal RiesgoPrima171 { get; set; }
    public decimal Provivienda2 { get; set; }
    public decimal AporteSolidario35 { get; set; }
    public decimal TotalAportes { get; set; }
}
