using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Horario")]
public class Horario
{
    [Key]
    [Column("id_horario")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHorario { get; set; }

    [Required(ErrorMessage = "El trabajador es obligatorio.")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "El campo dia semana es obligatorio.")]
    [Column("id_dia_semana")]
    public int IdDiaSemana { get; set; }

    [Required(ErrorMessage = "La hora de entrada es obligatoria.")]
    [Column("hora_entrada",TypeName = "time")]
    public TimeSpan HoraEntrada { get; set; }

    [Required(ErrorMessage = "La hora de salida es obligatoria.")]
    [Column("hora_salida",TypeName = "time")]
    public TimeSpan HoraSalida { get; set; }


    //FK
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

    [ForeignKey(nameof(IdDiaSemana))]
    public Clasificador DiaSemana { get; set; } = null!;
}

