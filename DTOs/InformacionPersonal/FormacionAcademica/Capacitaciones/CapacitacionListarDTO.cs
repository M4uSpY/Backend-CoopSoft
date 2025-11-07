using System;

namespace BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;

public class CapacitacionListarDTO
{
    public int IdCapacitacion { get; set; }
    public int IdTrabajador { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public byte[]? CertificadoArchivo { get; set; }
}
