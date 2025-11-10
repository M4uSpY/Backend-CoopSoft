using System;

namespace BackendCoopSoft.DTOs.Extras;

public class HorarioDTO
{
    public string DiaSemana { get; set; } = string.Empty;   
    public TimeSpan  HoraEntrada { get; set; }
    public TimeSpan  HoraSalida { get; set; }
}
