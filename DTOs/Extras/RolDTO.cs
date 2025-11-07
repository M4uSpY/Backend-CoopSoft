using System;

namespace BackendCoopSoft.DTOs.Extras;

public class RolDTO
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
