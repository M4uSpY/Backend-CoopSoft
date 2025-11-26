using System;

namespace BackendCoopSoft.DTOs.Historicos;

public class HistoricoFaltaListarDTO
{
    public int IdHistorico { get; set; }
    public int IdFalta { get; set; }
    public string UsuarioModifico { get; set; } = string.Empty;
    public DateTime FechaModificacion { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string ApartadosModificados { get; set; } = string.Empty;
}
