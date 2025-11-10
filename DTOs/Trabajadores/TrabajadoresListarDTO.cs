using System;
using BackendCoopSoft.DTOs.Extras;

namespace BackendCoopSoft.DTOs.Trabajadores;

public class TrabajadoresListarDTO
{
    public int IdTrabajador { get; set; }
    public string CI { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public int IdNacionalidad { get; set; }
    public bool Genero { get; set; }
    public string NombreOficina { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public List<HorarioDTO> Horarios { get; set; } = new List<HorarioDTO>();
}
