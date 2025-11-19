using System;

namespace BackendCoopSoft.DTOs.Extras;

public class LogsAccesoDTO
{
    public int IdLog { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;    
    public DateTime FechaLogin { get; set; }
    public TimeSpan HoraLogin { get; set; }
    public DateTime? FechaLogout { get; set; }
    public TimeSpan? HoraLogout { get; set; }
}