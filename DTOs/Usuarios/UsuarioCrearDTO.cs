using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.Usuarios;

public class UsuarioCrearDTO
{
    [Required]
    public int IdPersona { get; set; }

    [Required, StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int IdRol { get; set; }

    [Required]
    public bool EstadoUsuario { get; set; }
}
