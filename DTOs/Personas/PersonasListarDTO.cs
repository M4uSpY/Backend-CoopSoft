using System;
using BackendCoopSoft.DTOs.InformacionPersonal;
using BackendCoopSoft.Models;

namespace BackendCoopSoft.DTOs.Personas;

public class PersonasListarDTO
{
    public int IdPersona { get; set; }
    public string CarnetIdentidad { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public int IdNacionalidad { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[]? Foto { get; set; }
    public TrabajadorPersonaDTO? Trabajador { get; set; }
}
