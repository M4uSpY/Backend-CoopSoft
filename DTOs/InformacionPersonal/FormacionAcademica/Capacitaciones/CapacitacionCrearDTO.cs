using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;

public class CapacitacionCrearDTO
{
    [Required] 
    public int IdTrabajador { get; set; }
    [Required, StringLength(150)] 
    public string Titulo { get; set; } = string.Empty;
    [Required, StringLength(100)] 
    public string Institucion { get; set; } = string.Empty;
    public int CargaHoraria { get; set; }
    [Required] 
    public DateTime Fecha { get; set; }
    public byte[]? CertificadoArchivo { get; set; }
}
