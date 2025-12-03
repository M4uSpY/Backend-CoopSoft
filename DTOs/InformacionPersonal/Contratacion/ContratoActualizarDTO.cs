using System;

namespace BackendCoopSoft.DTOs.InformacionPersonal.Contratacion;

public class ContratoActualizarDTO
{
    public int IdContrato { get; set; }
    public int IdTipoContrato { get; set; }
    public int IdPeriodoPago { get; set; }

    public string NumeroContrato { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public byte[]? ArchivoPdf { get; set; }
}

