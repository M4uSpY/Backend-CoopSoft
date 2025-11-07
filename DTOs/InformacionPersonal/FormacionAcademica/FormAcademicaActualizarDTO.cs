using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica;

public class FormAcademicaActualizarDTO
{
    public int IdTrabajador { get; set; }
    [Required, StringLength(50)]
    public string NivelEstudios { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string TituloObtenido { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Institucion { get; set; } = string.Empty;
    public int AnioGraduacion { get; set; }
}
