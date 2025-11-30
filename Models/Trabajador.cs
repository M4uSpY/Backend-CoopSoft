using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Trabajador")]
public class Trabajador
{
    [Key]
    [Column("id_trabajador")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "Es necesario una persona")]
    [Column("id_persona")]
    public int IdPersona { get; set; }

    [Required(ErrorMessage = "Es necesario un cargo")]
    [Column("id_cargo")]
    public int IdCargo { get; set; }

    [Column("estado_trabajador")]
    public bool EstadoTrabajador { get; set; }

    [Required(ErrorMessage = "El haber basico es necesario")]
    [Column("haber_basico", TypeName = "decimal")]
    public decimal HaberBasico { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es necesaria")]
    [Column("fecha_ingreso", TypeName = "date")]
    public DateTime FechaIngreso { get; set; }

    //FK
    [ForeignKey(nameof(IdPersona))]
    [InverseProperty(nameof(Persona.Trabajador))] // opcional, clarifica el 1:1
    public Persona Persona { get; set; } = null!;

    [ForeignKey(nameof(IdCargo))]
    public Cargo Cargo { get; set; } = null!;

    // 1:N colecciones según el diagrama
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    public ICollection<Horario> Horarios { get; set; } = new List<Horario>();
    public ICollection<Capacitacion> Capacitaciones { get; set; } = new List<Capacitacion>();
    public ICollection<FormacionAcademica> FormacionesAcademicas { get; set; } = new List<FormacionAcademica>();
    public ICollection<Falta> Faltas { get; set; } = new List<Falta>();
    public ICollection<Vacacion> Vacaciones { get; set; } = new List<Vacacion>();
    public ICollection<Licencia> Licencias { get; set; } = new List<Licencia>();

    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    public ICollection<TrabajadorPlanilla> TrabajadorPlanillas { get; set; } = new List<TrabajadorPlanilla>();

    [InverseProperty(nameof(HistoricoTrabajador.Trabajador))]
    public ICollection<HistoricoTrabajador> HistoricosTrabajador { get; set; } = new List<HistoricoTrabajador>();

}

