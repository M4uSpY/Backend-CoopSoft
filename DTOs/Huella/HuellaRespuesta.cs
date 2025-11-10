using System;

namespace BackendCoopSoft.DTOs.Huella;

public class HuellaRespuesta
{
    public int IdPersona { get; set; }
    public string PrimerNombre { get; set; } = string.Empty;
    public string SegundoNombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string? TemplateXml { get; set; } = string.Empty;
}
