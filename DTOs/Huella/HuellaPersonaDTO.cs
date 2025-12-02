using System;

namespace BackendCoopSoft.DTOs.Huella;

public class HuellaPersonaDTO
{
    public int IdHuella { get; set; }
    public int IdPersona { get; set; }
    public int IndiceDedo { get; set; }
    public string TemplateXml { get; set; } = string.Empty;
}
