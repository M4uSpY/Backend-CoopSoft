using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.Licencias;

public class LicenciaCrearDTO
{
    [Required]
    public int IdTrabajador { get; set; }

    [Required]
    public int IdTipoLicencia { get; set; } // Clasificador: TipoLicencia

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }
    
    [Required]
    public TimeSpan HoraInicio { get; set; }

    [Required]
    public TimeSpan HoraFin { get; set; }


    [StringLength(200)]
    public string Motivo { get; set; } = string.Empty;

    public string? Observacion { get; set; }
}
