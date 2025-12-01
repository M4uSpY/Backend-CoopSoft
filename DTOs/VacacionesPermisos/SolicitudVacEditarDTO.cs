using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class SolicitudVacEditarDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}
