using System;

namespace BackendCoopSoft.DTOs.Extras;

public class HorarioDTO
{
    public int IdDiaSemana { get; set; }
    public TimeSpan HoraEntrada { get; set; }
    public TimeSpan HoraSalida { get; set; }
}
