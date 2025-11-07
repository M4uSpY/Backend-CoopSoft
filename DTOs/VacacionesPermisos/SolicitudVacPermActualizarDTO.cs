using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class SolicitudVacPermActualizarDTO
{
    [Required] public int IdSolicitud { get; set; }
    [Required] public int IdEstado { get; set; } // Clasificador: EstadoSolicitud
    public string? ObservacionRespuesta { get; set; }
}
