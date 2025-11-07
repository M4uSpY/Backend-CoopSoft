using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Trabajador_Planilla")]
public class TrabajadorPlanilla
{
    [Key]
    [Column("id_trabajador_planilla")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTrabajadorPlanilla { get; set; }

    [Required]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required]
    [Column("id_planilla")]
    public int IdPlanilla { get; set; }

    // Afiliaciones y detalle del mes
    [Column("es_aportante")] public bool EsAportante { get; set; }
    [Column("afiliado_gestora")] public bool AfiliadoGestora { get; set; }
    [Column("afiliado_caja")] public bool AfiliadoCaja { get; set; }
    [Column("afiliado_provivenda")] public bool AfiliadoProvivienda { get; set; }

    [StringLength(100)]
    [Column("nombre_cargo_mes", TypeName = "nvarchar(100)")]
    public string NombreCargoMes { get; set; } = string.Empty;

    [Column("haber_basico_mes", TypeName = "decimal(18,2)")]
    public decimal HaberBasicoMes { get; set; }

    [Column("dias_trabajados")]   public int DiasTrabajados { get; set; }
    [Column("horas_trabajadas")]  public int HorasTrabajadas { get; set; }
    [Column("antiguedad_meses")]  public int AntiguedadMeses { get; set; }

    // Navs
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

    [ForeignKey(nameof(IdPlanilla))]
    public Planilla Planilla { get; set; } = null!;

    public ICollection<HistoricoTrabajadorPlanilla> HistoricoTrabajadorPlanillas { get; set; } = new List<HistoricoTrabajadorPlanilla>();
    public ICollection<TrabajadorPlanillaValor> TrabajadorPlanillaValors { get; set; } = new List<TrabajadorPlanillaValor>();
}

