using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class CalendarioEventoDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
