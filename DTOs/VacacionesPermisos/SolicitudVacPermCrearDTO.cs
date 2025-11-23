using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class SolicitudVacPermCrearDTO
{
    [Required] 
    public int IdTrabajador { get; set; }
    [Required] 
    public DateTime FechaInicio { get; set; }
    [Required] 
    public DateTime FechaFin { get; set; }


    [Required, StringLength(150)] 
    public string Motivo { get; set; } = string.Empty;
    
    public string? Observacion { get; set; }
}
