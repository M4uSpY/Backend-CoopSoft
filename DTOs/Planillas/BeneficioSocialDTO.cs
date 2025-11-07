using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class BeneficioSocialDTO
{
    public int IdTrabajadorPlanilla { get; set; }
    public decimal Indemnizacion { get; set; }
    public decimal Desahucio { get; set; }
    public decimal Vacaciones { get; set; }
    public decimal Total { get; set; }
}
