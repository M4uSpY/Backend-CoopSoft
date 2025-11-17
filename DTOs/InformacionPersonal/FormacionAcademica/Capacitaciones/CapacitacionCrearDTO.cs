using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;

public class CapacitacionCrearDTO
{
    public int IdTrabajador { get; set; }
    
    public string Titulo { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public int CargaHoraria { get; set; }
    public DateTime Fecha { get; set; }
    public byte[]? ArchivoCertificado { get; set; }
}
