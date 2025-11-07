using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Trabajador_Planilla_Valor")]
public class TrabajadorPlanillaValor
{
    [Key]
    [Column("id_trabajador_planilla_valor")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTrabajadorPlanillaValor { get; set; }

    [Required]
    [Column("id_trabajador_planilla")]
    public int IdTrabajadorPlanilla  { get; set; }

    [Required]
    [Column("id_concepto")]
    public int IdConcepto { get; set; }

    [Required]
    [Column("valor", TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal Valor { get; set; }          // siempre en Bs

    // NUEVO: quién puso el dato
    [Column("es_manual")]
    public bool EsManual { get; set; }          // true si el usuario lo escribió (ej. RC-IVA)

    [Column("observacion", TypeName = "nvarchar(300)")]
    public string? Observacion { get; set; }    // motivo del override / nota

    // Navs
    [ForeignKey(nameof(IdTrabajadorPlanilla))]
    public TrabajadorPlanilla TrabajadorPlanilla { get; set; } = null!;

    [ForeignKey(nameof(IdConcepto))]
    public Concepto Concepto { get; set; } = null!;
}
