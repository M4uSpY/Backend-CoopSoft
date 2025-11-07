using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class CalendarioEventosDTO
{
    public int IdTrabajador { get; set; }
    public List<CalendarioEventoDTO> Eventos { get; set; } = new();
}
