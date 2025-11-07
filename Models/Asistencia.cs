using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;


[Table("Asistencia")]
public class Asistencia
{
    [Key]
    [Column("id_asistencia")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdAsistencia { get; set; }

    [Required(ErrorMessage = "El trabajador es obligatorio")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    [Column("fecha", TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "La hora es obligatoria")]
    [Column("hora", TypeName = "time")]
    public TimeSpan Hora { get; set; }

    [Column("es_entrada", TypeName = "bit")]
    public bool EsEntrada { get; set; }


    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

}
