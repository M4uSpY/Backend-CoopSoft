using System;

namespace BackendCoopSoft.DTOs.Planillas;

public class AguinaldoDTO
{
    public int IdTrabajadorPlanilla { get; set; }
    public decimal PromedioUltimosTresMeses { get; set; }
    public decimal AguinaldoCalculado { get; set; }
}
