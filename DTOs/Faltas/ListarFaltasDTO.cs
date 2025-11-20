using System;

namespace BackendCoopSoft.DTOs.Faltas;

public class ListarFaltasDTO
{
    public int IdFalta { get; set; }
    public string CI { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;

    // Tabla clasificador
    public int Tipo { get; set; }
    
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public byte[]? ArchivoJustificativo { get; set; }
}
