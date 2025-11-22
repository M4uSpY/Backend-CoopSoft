using System;

namespace BackendCoopSoft.DTOs.VacacionesPermisos;

public class ResumenVacacionesDTO
{
    public int Gestion { get; set; }
    public DateTime FechaIngreso { get; set; }
    public int AntiguedadAnios { get; set; }

    public int DiasDerecho { get; set; }
    public int DiasUsados { get; set; }
    public int DiasDisponibles { get; set; }
}
