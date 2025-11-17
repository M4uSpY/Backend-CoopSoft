using System;
using BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica;
using BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;

namespace BackendCoopSoft.DTOs.InformacionPersonal;

public class TrabajadorPersonaDTO
{
    public int IdTrabajador { get; set; }
    public DateTime FechaIngreso { get; set; }

    public string? TituloObtenido { get; set; }

    public List<FormacionAcademicaResumenDTO> Formaciones { get; set; } = new();
    public List<CapacitacionResumenDTO> Capacitaciones { get; set; } = new();

}
