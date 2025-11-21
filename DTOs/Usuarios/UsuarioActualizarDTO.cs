using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.Usuarios;

public class UsuarioActualizarDTO
{
    [Required]
    public int IdPersona { get; set; }

    [Required, StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public int IdRol { get; set; }

    // Contraseña opcional SOLO para cambio
    public string? PasswordNueva { get; set; }
}

