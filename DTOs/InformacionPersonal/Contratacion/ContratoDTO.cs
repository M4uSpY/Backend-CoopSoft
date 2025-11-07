using System;

namespace BackendCoopSoft.DTOs.InformacionPersonal.Contratacion;

public class ContratoDTO
{
    public int IdContrato { get; set; }
    public int IdTrabajador { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public int IdTipoContrato { get; set; } // Clasificador
    public int IdPeriodoPago { get; set; } // Clasificador
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? UrlArchivo { get; set; } // para DESCARGAR CONTRATO
}
