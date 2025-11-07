using System;

namespace BackendCoopSoft.DTOs.Usuarios;

public class UsuarioListarDTO
{
    public int IdUsuario { get; set; }
    public string CI { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string DescripcionRol { get; set; } = string.Empty;
    public int IdPersona { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string Genero { get; set; } = string.Empty;
}
