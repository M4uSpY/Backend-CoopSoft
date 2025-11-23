using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class PlanillaCrearDTO
{
    public int IdTipoPlanilla { get; set; }   // Sueldos, Aguinaldo, etc.
    public int Gestion { get; set; }          // 2025
    public int Mes { get; set; }              // 1..12
    public DateTime PeriodoDesde { get; set; }
    public DateTime PeriodoHasta { get; set; }
}
