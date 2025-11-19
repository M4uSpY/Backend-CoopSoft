using System;

namespace BackendCoopSoft.DTOs;

public class AuthRespuestaDTO
{
    public string Token { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int IdPersona { get; set; }
    public int IdUsuario { get; set; }
    public string Rol { get; set; } = string.Empty;
}
