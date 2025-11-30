using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Historico_Trabajador")]
public class HistoricoTrabajador
{
    [Key]
    [Column("id_historico")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHistorico { get; set; }

    [Required]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required]
    [Column("usuario_modifico")]
    public int UsuarioModificoId { get; set; }

    [Column("fecha_modificacion", TypeName = "date")]
    public DateTime FechaModificacion { get; set; }

    [Required]
    [StringLength(30)]
    [Column("accion")]
    public string Accion { get; set; } = string.Empty; // CREAR / ACTUALIZAR / INACTIVAR

    [Required]
    [Column("campo", TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string Campo { get; set; } = string.Empty;

    [Column("valor_anterior", TypeName = "nvarchar(max)")]
    public string? ValorAnterior { get; set; }

    [Column("valor_actual", TypeName = "nvarchar(max)")]
    public string? ValorActual { get; set; }

    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

    [ForeignKey(nameof(UsuarioModificoId))]
    public Usuario UsuarioModifico { get; set; } = null!;
}

