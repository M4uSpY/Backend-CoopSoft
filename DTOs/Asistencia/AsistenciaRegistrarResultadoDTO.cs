using System;

namespace BackendCoopSoft.DTOs.Asistencia;

public class AsistenciaRegistrarResultadoDTO
{
    public bool Registrado { get; set; }
    public bool EsEntrada { get; set; }
    public bool FaltaGenerada { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
