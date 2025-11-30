using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Historico_Usuario")]
public class HistoricoUsuario
{
    [Key]
    [Column("id_historico")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHistorico { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [Column("usuario_modifico")]
    public int UsuarioModificoId { get; set; }

    [Column("fecha_modificacion", TypeName = "datetime")]
    public DateTime FechaModificacion { get; set; }

    [Required]
    [Column("accion", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string Accion { get; set; } = string.Empty;

    [Required]
    [Column("campo", TypeName = "varchar(100)")]
    [StringLength(100)]
    public string Campo { get; set; } = string.Empty;  // "NombreUsuario", "EstadoUsuario", etc.

    [Column("valor_anterior", TypeName = "varchar(max)")]
    public string? ValorAnterior { get; set; }

    [Column("valor_actual", TypeName = "varchar(max)")]
    public string? ValorActual { get; set; }

    // 🔗 Propiedad de navegación al usuario "dueño" del histórico
    [ForeignKey(nameof(IdUsuario))]
    [InverseProperty(nameof(Usuario.HistoricosComoTitular))]
    public Usuario Usuario { get; set; } = null!;

    // 🔗 Propiedad de navegación al usuario que hizo la modificación
    [ForeignKey(nameof(UsuarioModificoId))]
    [InverseProperty(nameof(Usuario.HistoricosModificadosPorMi))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

