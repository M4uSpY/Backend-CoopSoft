using System;

namespace BackendCoopSoft.DTOs.Faltas;

// POR VER SI TENDRA EL BOTON ACTUALZIAR
public class ActualizarFaltaDTO
{
    public int IdFalta { get; set; }
    public int IdTipoFalta { get; set; }
    public string? Descripcion { get; set; }
}
