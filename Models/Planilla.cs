using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Planilla")]
public class Planilla
{
    [Key]
    [Column("id_planilla")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPlanilla { get; set; }

    // NUEVO: tipo por Clasificador (SUELDOS_SALARIOS, APORTES_PATRONALES, AGUINALDO, PRIMA)
    [Required]
    [Column("id_tipo_planilla")]
    public int IdTipoPlanilla { get; set; }

    [Required]
    [Column("gestion")]
    public int Gestion { get; set; }

    [Required]
    [Column("mes")]
    public int Mes { get; set; }

    // NUEVO: rango del periodo
    [Required]
    [Column("periodo_desde", TypeName = "date")]
    public DateTime PeriodoDesde { get; set; }

    [Required]
    [Column("periodo_hasta", TypeName = "date")]
    public DateTime PeriodoHasta { get; set; }

    // NUEVO: estado simple abierto/cerrado
    [Column("esta_cerrada")]
    public bool EstaCerrada { get; set; }

    [Column("fecha_cierre", TypeName = "date")]
    public DateTime? FechaCierre { get; set; }

    // Navs
    [ForeignKey(nameof(IdTipoPlanilla))]
    [InverseProperty(nameof(Clasificador.TiposPlanilla))]
    public Clasificador TipoPlanilla { get; set; } = null!;

    public ICollection<TrabajadorPlanilla> TrabajadorPlanillas { get; set; } = new List<TrabajadorPlanilla>();
    public ICollection<HistoricoPlanilla> HistoricosPlanillaSueldos { get; set; } = new List<HistoricoPlanilla>();
}

