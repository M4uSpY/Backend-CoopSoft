using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Solicitud")]
public class Solicitud
{
    [Key]
    [Column("id_solicitud")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdSolicitud { get; set; }

    [Required(ErrorMessage = "Es necesario un trabajador")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "Es necesario un estado de la Solicitud")]
    [Column("id_estado_solicitud")]
    public int IdEstadoSolicitud { get; set; }

    [Required(ErrorMessage = "Es necesario indicar el motivo")]
    [Column("motivo", TypeName = "nvarchar(max)")]
    public string Motivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio de la solicitud si es requerida.")]
    [Column("fecha_inicio", TypeName = "date")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha final de la solicitud si es requerida.")]
    [Column("fecha_fin", TypeName = "date")]
    public DateTime FechaFin { get; set; }

    [Column("observacion", TypeName = "nvarchar(max)")]
    public string? Observacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario tener la fecha de emision de la solicitud")]
    [Column("fecha_solicitud", TypeName = "date")]
    public DateTime FechaSolicitud { get; set; }

    [Column("fecha_aprobacion", TypeName = "date")]
    public DateTime? FechaAprobacion { get; set; }

    // FK
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;


    [ForeignKey(nameof(IdEstadoSolicitud))]
    [InverseProperty(nameof(Clasificador.EstadosSolicitud))]
    public Clasificador EstadoSolicitud { get; set; } = null!;
}
