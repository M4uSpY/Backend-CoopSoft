using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Historico_Falta")]
public class HistoricoFalta
{
    [Key]
    [Column("id_historico")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHistorico { get; set; }

    [Required]
    [Column("id_falta")]
    public int IdFalta { get; set; }

    [Required]
    [Column("usuario_modifico")]
    public int UsuarioModificoId { get; set; }

    [Column("fecha_modificacion", TypeName = "date")]
    public DateTime FechaModificacion { get; set; }

    // 🔗 FK -> Falta
    [ForeignKey(nameof(IdFalta))]
    public Falta Falta { get; set; } = null!;

    // 🔗 FK -> Usuario (quien modificó)
    [ForeignKey(nameof(UsuarioModificoId))]
    [InverseProperty(nameof(Usuario.HistoricosFaltaModificadosPorMi))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

