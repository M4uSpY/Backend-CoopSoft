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

    [Column("fecha_modificacion", TypeName = "date")]
    public DateTime FechaModificacion { get; set; }

    // 🔗 Propiedad de navegación al usuario "dueño" del histórico
    [ForeignKey(nameof(IdUsuario))]
    [InverseProperty(nameof(Usuario.HistoricosComoTitular))]
    public Usuario Usuario { get; set; } = null!;

    // 🔗 Propiedad de navegación al usuario que hizo la modificación
    [ForeignKey(nameof(UsuarioModificoId))]
    [InverseProperty(nameof(Usuario.HistoricosModificadosPorMi))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

