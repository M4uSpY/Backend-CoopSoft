using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs;

public class LoginSolicitudDTO
{
    [Required]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
