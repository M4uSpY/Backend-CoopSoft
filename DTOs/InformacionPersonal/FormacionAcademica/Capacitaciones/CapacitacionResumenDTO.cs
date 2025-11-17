using System;

namespace BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;

public class CapacitacionResumenDTO
{
    public int IdCapacitacion { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public byte[]? ArchivoCertificado { get; set; }
}
