using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.Personas;

public class PersonaCrearDTO
{
    [Required]
    public int IdNacionalidad { get; set; }

    [Required, StringLength(50)]
    public string PrimerNombre { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SegundoNombre { get; set; }

    [Required, StringLength(50)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string CarnetIdentidad { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }

    [Required]
    public bool Genero { get; set; }

    [Required, StringLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    public byte[]? Foto { get; set; }
    
    public byte[]? Huella { get; set; }
}
