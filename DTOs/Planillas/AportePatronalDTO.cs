using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class AportePatronalDTO
{
    public int IdTrabajadorPlanilla { get; set; }
    public decimal CajaSalud { get; set; }
    public decimal AFPPatronal { get; set; }
    public decimal RiesgoProfesional { get; set; }
    public decimal Total { get; set; }
}   
