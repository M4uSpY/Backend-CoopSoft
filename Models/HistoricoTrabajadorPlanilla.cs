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

    [Required]
    [Column("accion", TypeName = "nvarchar(50)")]
    public string Accion { get; set; } = string.Empty;

    [Required]
    [Column("campo", TypeName = "nvarchar(100)")]
    public string Campo { get; set; } = string.Empty;

    [Column("valor_anterior", TypeName = "nvarchar(max)")]
    public string? ValorAnterior { get; set; }

    [Column("valor_actual", TypeName = "nvarchar(max)")]
    public string? ValorActual { get; set; }

    

    [ForeignKey(nameof(IdTrabajadorPlanilla))]
    public TrabajadorPlanilla TrabajadorPlanilla { get; set; } = null!;

    [ForeignKey(nameof(UsuarioModificoId))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

