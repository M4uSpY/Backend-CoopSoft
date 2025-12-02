using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Formacion_Academica")]
public class FormacionAcademica
{
    [Key]
    [Column("id_formacion")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdFormacion { get; set; }

    [Required(ErrorMessage = "El trabajador es obligatorio.")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "El nivel de estudios es obligatorio.")]
    [StringLength(50)]
    [Column("nivel_estudios", TypeName = "nvarchar(50)")]
    public string NivelEstudios { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título obtenido es obligatorio.")]
    [StringLength(100)]
    [Column("titulo_obtenido", TypeName = "nvarchar(100)")]
    public string TituloObtenido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(100)]
    [Column("institucion", TypeName = "nvarchar(100)")]
    public string Institucion { get; set; } = string.Empty;

    [Column("anio_graduacion")]
    public int AnioGraduacion { get; set; }

    [Required(ErrorMessage = "El archivo pdf es requerido")]
    [Column("archivo_pdf", TypeName = "varbinary(max)")]
    public byte[] ArchivoPdf { get; set; } = Array.Empty<byte>();

    [StringLength(50)]
    [Column("nro_registro_profesional", TypeName = "varchar(50)")]
    public string? NroRegistroProfesional { get; set; }

    // 🔗 Propiedad de navegación
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;
}

