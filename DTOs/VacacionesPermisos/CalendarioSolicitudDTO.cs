using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class CalendarioSolicitudDTO
{
    public int Id { get; set; }
    public string Trabajador { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;  // Vacación / Permiso
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estatus { get; set; } = string.Empty;
}
