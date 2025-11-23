using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackendCoopSoft.Models;

[Table("Licencia")]
public class Licencia
{
    [Key]
    [Column("id_licencia")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdLicencia { get; set; }

    [Required]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required]
    [Column("id_tipo_licencia")]
    public int IdTipoLicencia { get; set; } // Clasificador: TipoLicencia

    // ⚠️ NUEVO: estado de la licencia (Pendiente, Aprobada, Rechazada)
    [Required]
    [Column("id_estado_licencia")]
    public int IdEstadoLicencia { get; set; }  // reutilizamos EstadoSolicitud

    [Required]
    [Column("fecha_inicio", TypeName = "date")]
    public DateTime FechaInicio { get; set; }

    [Required]
    [Column("fecha_fin", TypeName = "date")]
    public DateTime FechaFin { get; set; }

    [Required]
    [Column("hora_inicio", TypeName = "time")]
    public TimeSpan HoraInicio { get; set; }

    [Required]
    [Column("hora_fin", TypeName = "time")]
    public TimeSpan HoraFin { get; set; }

    // equivalente en jornadas (0.5, 1, 2, 3, etc.)
    [Required]
    [Column("cantidad_jornadas", TypeName = "decimal(5,2)")]
    public decimal CantidadJornadas { get; set; }

    [Column("motivo", TypeName = "nvarchar(200)")]
    public string Motivo { get; set; } = string.Empty;

    [Column("observacion", TypeName = "nvarchar(max)")]
    public string? Observacion { get; set; }

    [Column("archivo_justificativo", TypeName = "varbinary(max)")]
    public byte[] ArchivoJustificativo { get; set; } = Array.Empty<byte>();

    [Required]
    [Column("fecha_registro", TypeName = "datetime2")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Column("fecha_aprobacion", TypeName = "date")]
    public DateTime? FechaAprobacion { get; set; }

    // FK
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

    [ForeignKey(nameof(IdTipoLicencia))]
    [InverseProperty(nameof(Clasificador.Licencias))]
    public Clasificador TipoLicencia { get; set; } = null!;

    // ⚠️ NUEVA NAV: usamos la misma categoría "EstadoSolicitud"
    [ForeignKey(nameof(IdEstadoLicencia))]
    [InverseProperty(nameof(Clasificador.EstadosLicencia))]
    public Clasificador EstadoLicencia { get; set; } = null!;
}
