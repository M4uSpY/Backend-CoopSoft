using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class CalendarioTrabajadorDTO
{
    public int IdTrabajador { get; set; }
    public List<SolicitudCalendarioDTO> Solicitudes { get; set; } = new();
}
