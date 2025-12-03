using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Persona")]
public class Persona
{
    [Key]
    [Column("id_persona")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPersona { get; set; }

    [Required(ErrorMessage = "Es necesario el apartado nacionalidad")]
    [Column("id_nacionalidad")]
    public int IdNacionalidad { get; set; }

    [StringLength(50)]
    [Column("primer_nombre", TypeName = "nvarchar(50)")]
    public string PrimerNombre { get; set; } = string.Empty;

    [StringLength(50)]
    [Column("segundo_nombre", TypeName = "nvarchar(50)")]
    public string? SegundoNombre { get; set; }

    [Required(ErrorMessage = "Es necesario el apellido paterno")]
    [StringLength(50, ErrorMessage = "No debe exceder de los 50 caracteres")]
    [Column("apellido_paterno", TypeName = "nvarchar(50)")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario el apellido materno")]
    [StringLength(50, ErrorMessage = "No debe exceder de los 50 caracteres")]
    [Column("apellido_materno", TypeName = "nvarchar(50)")]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario el carnet de identidad")]
    [StringLength(20, ErrorMessage = "No debe exceder de los 20 caracteres")]
    [Column("carnet_identidad", TypeName = "varchar(20)")]
    public string CarnetIdentidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario la fecha de nacimiento paterno")]
    [Column("fecha_nacimiento", TypeName = "date")]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "Es necesario el genero")]
    [Column("genero")]
    public bool Genero { get; set; }

    [Required(ErrorMessage = "Es necesario la direccion de domicilio")]
    [Column("direccion", TypeName = "nvarchar(250)")]
    [StringLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario tener un estado (activo o inactivo)")]
    [Column("estado_persona")]
    public bool EstadoPersona { get; set; } = true;


    [Required(ErrorMessage = "Es necesario un numero de telefono")]
    [Column("telefono", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario un correo electronico")]
    [Column("email", TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario una foto de la Persona")]
    [Column("foto", TypeName = "varbinary(max)")]
    public byte[]? Foto { get; set; }

    // FK
    [ForeignKey(nameof(IdNacionalidad))]
    public Clasificador Nacionalidad { get; set; } = null!;

    //1:N
    public ICollection<HuellaDactilar> HuellaDactilars { get; set; } = new List<HuellaDactilar>();

    // 1:1 obligatorio Usuario
    [InverseProperty(nameof(Usuario.Persona))]
    public Usuario Usuario { get; set; } = null!;

    //1:N Historico_Persona
    public ICollection<HistoricoPersona> HistoricoPersonas { get; set; } = new List<HistoricoPersona>();

    // 1:1 puede o no ser trabajador
    [InverseProperty(nameof(Trabajador.Persona))]
    public Trabajador? Trabajador { get; set; }

}

