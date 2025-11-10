using System;

namespace BackendCoopSoft.DTOs.Huella;

public class HuellaDTO
{
    public int IdPersona { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string? TemplateXml { get; set; }
}
