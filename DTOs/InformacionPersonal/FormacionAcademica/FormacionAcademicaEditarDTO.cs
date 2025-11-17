using System;

namespace BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica;

public class FormacionAcademicaEditarDTO
{
    public int IdFormacion { get; set; }      // viene de la BD
    public int IdTrabajador { get; set; }

    public string NivelEstudios { get; set; } = string.Empty;
    public string TituloObtenido { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public int AnioGraduacion { get; set; }
    public string? NroRegistroProfesional { get; set; }
}

