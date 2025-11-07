using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Usuario")]
public class Usuario
{
    [Key]
    [Column("id_usuario")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "Es necesario tener una persona.")]
    [Column("id_persona")]
    public int IdPersona { get; set; }

    [Required(ErrorMessage = "Es necesario tener un rol.")]
    [Column("id_rol")]
    public int IdRol { get; set; }

    [Required(ErrorMessage = "Es necesario tener un estado (activo o inactivo)")]
    [Column("estado_usuario")]
    public bool EstadoUsuario { get; set; }

    [Required(ErrorMessage = "Es necesario indicar un nombre de usuario")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no debe exceder los 50 caracteres")]
    [Column("nombre_usuario", TypeName = "varchar(50)")] // esto colocar Unique en el DBCONTEXT
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario instanciar un password.")]
    [Column("password", TypeName = "varchar(255)")]
    public string Password { get; set; } = string.Empty;

    //FK
    [ForeignKey(nameof(IdPersona))]
    [InverseProperty(nameof(Persona.Usuario))] // opcional, clarifica el 1:1
    public Persona Persona { get; set; } = null!;

    [ForeignKey(nameof(IdRol))]
    public Rol Rol { get; set; } = null!;

    // RELACIONES
    // Relacion (1 Usuario -> N HistoricoUsuario)
    [InverseProperty(nameof(HistoricoUsuario.Usuario))]
    public ICollection<HistoricoUsuario> HistoricosComoTitular { get; set; } = new List<HistoricoUsuario>();

    [InverseProperty(nameof(HistoricoUsuario.UsuarioModifico))]
    public ICollection<HistoricoUsuario> HistoricosModificadosPorMi { get; set; } = new List<HistoricoUsuario>();

    // Relacion (1 Usuario -> N LogAcceso)
    public ICollection<LogAcceso> LogAccesos { get; set; } = new List<LogAcceso>();

    // Historico_Falta (actor = este usuario)
    [InverseProperty(nameof(HistoricoFalta.UsuarioModifico))]
    public ICollection<HistoricoFalta> HistoricosFaltaModificadosPorMi { get; set; } = new List<HistoricoFalta>();

    // Historico_Planilla_Sueldos (actor = este usuario)
    [InverseProperty(nameof(HistoricoPlanilla.UsuarioModifico))]
    public ICollection<HistoricoPlanilla> HistoricosPlanillaModificadosPorMi { get; set; } = new List<HistoricoPlanilla>();

    // Historico_Trabajador_Planilla (actor = este usuario)
    [InverseProperty(nameof(HistoricoTrabajadorPlanilla.UsuarioModifico))]
    public ICollection<HistoricoTrabajadorPlanilla> HistoricosTrabajadorPlanillaModificadosPorMi { get; set; } = new List<HistoricoTrabajadorPlanilla>();

    // Historico_Trabajador (actor = este usuario)
    [InverseProperty(nameof(HistoricoTrabajador.UsuarioModifico))]
    public ICollection<HistoricoTrabajador> HistoricosTrabajadorModificadosPorMi { get; set; } = new List<HistoricoTrabajador>();

    // Para navegar los históricos de persona modificados por este usuario:
    [InverseProperty(nameof(HistoricoPersona.UsuarioModifico))]
    public ICollection<HistoricoPersona> HistoricosPersonaModificadosPorMi { get; set; } = new List<HistoricoPersona>();

}
