using System;

namespace BackendCoopSoft.DTOs.Trabajadores;

public class TrabajadoresListarDTO
{
    public int IdTrabajador { get; set; }
    public string CI { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public bool Genero { get; set; }
    public int NroOficina { get; set; }
    public bool Activo { get; set; }
}
