using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.Faltas;

public class CrearFaltaDTO
{
    [Required]
    public int IdTrabajador { get; set; }
    [Required]
    public DateTime Fecha { get; set; }
    [Required]
    public string Descripcion { get; set; } = string.Empty;
    [Required]
    // Tabla clasificador
    public int IdTipoFalta { get; set; }
    public byte[]? ArchivoJustificativo { get; set; }
}
