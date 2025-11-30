using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class SolicitudVacListarDTO
{   
    public int IdVacacion { get; set; }
    public string CI { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Vacacion";


    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty; // Pendiente/Aprobado/Rechazado
}
