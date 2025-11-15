using System;

namespace BackendCoopSoft.DTOs.Asistencia;

public class AsistenciaCrearDTO
{
    public int IdTrabajador { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public bool esEntrada { get; set; }
}
