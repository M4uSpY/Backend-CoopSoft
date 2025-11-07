using System;

namespace BackendCoopSoft.DTOs.Extras;

public class CargoDTO
{
    public int IdCargo { get; set; }
    public int IdOficina { get; set; }
    public string NombreCargo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
