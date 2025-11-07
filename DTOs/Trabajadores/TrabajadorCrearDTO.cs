using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.Trabajadores;

public class TrabajadorCrearDTO
{
    [Required]
    public int IdPersona { get; set; }
    [Required]
    public decimal HaberBasico { get; set; }
    [Required]
    public DateTime FechaIngreso { get; set; }
    public int IdCargo { get; set; }
    public List<Extras.HorarioDTO> Horarios { get; set; } = new List<Extras.HorarioDTO>();

}
