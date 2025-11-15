using System;

namespace BackendCoopSoft.DTOs.Asistencia;

public class AsistenciaListaDTO
{
    public int IdAsistencia { get; set; }
    public string CI { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public string Oficina { get; set; } = string.Empty;
    public bool EsEntrada { get; set; } 
}
