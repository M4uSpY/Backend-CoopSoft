using System;

namespace BackendCoopSoft.DTOs.Historicos;

public class HistoricoUsuarioListarDTO
{
    public int IdHistorico { get; set; }
    public int IdUsuario { get; set; }
    public string UsuarioModifico { get; set; } = string.Empty;
    public DateTime FechaModificacion { get; set; }
    public string Accion { get; set; } = string.Empty;

    // Nuevo modelo
    public string Campo { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorActual { get; set; }
}
