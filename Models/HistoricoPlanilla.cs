using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Historico_Planilla")]
public class HistoricoPlanilla
{
    [Key]
    [Column("id_historico")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHistorico { get; set; }

    [Required]
    [Column("id_planilla")]
    public int IdPlanilla { get; set; }

    [Required]
    [Column("usuario_modifico")]
    public int UsuarioModificoId { get; set; }

    [Column("fecha_modificacion", TypeName = "date")]
    public DateTime FechaModificacion { get; set; }

    // 🔗 Propiedad de navegación al usuario "dueño" del histórico
    [ForeignKey(nameof(IdPlanilla))]
    public Planilla Planilla { get; set; } = null!;

    // 🔗 Propiedad de navegación al usuario que hizo la modificación
    [ForeignKey(nameof(UsuarioModificoId))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

