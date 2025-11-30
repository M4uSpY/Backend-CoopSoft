using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class SolicitudCalendarioDTO
{
    public int IdVacacion { get; set; }
    public string Trabajador { get; set; } = string.Empty;
    public string TipoSolicitud { get; set; } = string.Empty;
    public string EstadoSolicitud { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
