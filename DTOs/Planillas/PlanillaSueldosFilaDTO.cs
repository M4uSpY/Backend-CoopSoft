using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class PlanillaSueldosFilaDTO
{
    public int Id { get; set; }                    // No (1,2,3,...)

    public int IdTrabajadorPlanilla { get; set; }

    public string CarnetIdentidad { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string Ocupacion { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }

    public int DiasPagados { get; set; }

    // OTROS PAGOS
    public decimal HaberBasico { get; set; }
    public decimal BonoAntiguedad { get; set; }
    public decimal BonoProduccion { get; set; }
    public decimal AporteCoop334 { get; set; }
    public decimal TotalGanado { get; set; }

    // DESCUENTOS
    public decimal Gestora1221 { get; set; }
    public decimal RcIva13 { get; set; }
    public decimal AporteSolidario05 { get; set; }
    public decimal OtrosDesc668 { get; set; }
    public decimal OtrosDescuentos { get; set; }
    public decimal TotalDescuentos { get; set; }

    public decimal LiquidoPagable { get; set; }

    // La firma del empleado será solo espacio en blanco / texto
    public string FirmaEmpleado { get; set; } = string.Empty;
}
