using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class PlanillaDetalleDTO
{
    public int IdTrabajadorPlanilla { get; set; }
    public string CI { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public int DiasTrabajados { get; set; }
    public decimal HaberBasicoMes { get; set; }
    public int HorasTrabajadas { get; set; }
    public int AntiguedadMeses { get; set; }
    public List<ConceptoPlanillaDTO> Conceptos { get; set; } = new();
    public decimal TotalGanado { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal LiquidoPagable { get; set; }
}
