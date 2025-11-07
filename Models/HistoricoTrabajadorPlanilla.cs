    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Historico_Trabajador_Planilla")]
public class HistoricoTrabajadorPlanilla
{
    [Key]
    [Column("id_historico")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHistorico { get; set; }

    [Required]
    [Column("id_trabajador_planilla")]
    public int IdTrabajadorPlanilla { get; set; }

    [Required]
    [Column("usuario_modifico")]
    public int UsuarioModificoId { get; set; }

    [Column("fecha_modificacion", TypeName = "date")]
    public DateTime FechaModificacion { get; set; }

    // 🔗 FK -> TrabajadorPlanilla
    [ForeignKey(nameof(IdTrabajadorPlanilla))]
    public TrabajadorPlanilla TrabajadorPlanilla { get; set; } = null!;

    // 🔗 FK -> Usuario (quien modificó)
    [ForeignKey(nameof(UsuarioModificoId))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

