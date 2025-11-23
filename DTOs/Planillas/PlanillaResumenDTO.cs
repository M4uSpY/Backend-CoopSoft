using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class PlanillaResumenDTO
{
    public int IdPlanilla { get; set; }
    public int IdTipoPlanilla { get; set; }
    public int Gestion { get; set; }
    public int Mes { get; set; }
    public DateTime PeriodoDesde { get; set; }
    public DateTime PeriodoHasta { get; set; }
    public bool EstaCerrada { get; set; }
}
