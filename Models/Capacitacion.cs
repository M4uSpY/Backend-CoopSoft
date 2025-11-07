using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Capacitacion")]
public class Capacitacion
{
    [Key]
    [Column("id_capacitacion")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCapacitacion { get; set; }

    [Required(ErrorMessage = "Es necesario el trabajador")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [StringLength(100)]
    [Required(ErrorMessage = "Es necesario el titulo")]
    [Column("titulo", TypeName = "nvarchar(100)")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(100)]
    [Required(ErrorMessage = "Es necesario la Institucion")]
    [Column("institucion", TypeName = "nvarchar(100)")]
    public string Institucion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario la fecha")]
    [Column("fecha", TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "Es necesario la carga horaria")]
    [Column("carga_horaria")]
    public int CargaHoraria { get; set; }

    [Required(ErrorMessage = "Es necesario el archivo de la capacitacion")]
    [Column("archivo_certificado", TypeName = "varbinary(max)")]
    public byte[] ArchivoCertificado { get; set; } = Array.Empty<byte>();

    // FK
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;
}
