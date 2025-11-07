using System;

namespace BackendCoopSoft.DTOs.Extras;

public class OficinaDTO
{
    public int IdOficina { get; set; }
    public string NombreOficina { get; set; } = string.Empty;
    public string? Direccion { get; set; }
}
