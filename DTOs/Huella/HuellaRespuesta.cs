using System;

namespace BackendCoopSoft.DTOs.Huella;

public class HuellaRespuesta
{
    public int IdPersona { get; set; }
    public int IdTrabajador { get; set; }
    public string PrimerNombre { get; set; } = string.Empty;
    public string SegundoNombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string CI { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public byte[]? Foto { get; set; }
    public string? TemplateXml { get; set; } = string.Empty;

    public int IndiceDedo { get; set; }
}
