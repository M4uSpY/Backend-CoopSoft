using System;

namespace BackendCoopSoft.DTOs.InformacionPersonal;

public class DatosPersonalesDTO
{
    public string?  UrlFoto { get; set; }
    public string PrimerNombre { get; set; } = string.Empty;
    public string SegundoNombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string CI { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Email { get; set; } = string.Empty;
}
