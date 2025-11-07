using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Concepto")]
public class Concepto
{
    [Key]
    [Column("id_concepto")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdConcepto { get; set; }

    [Required, StringLength(100)]
    [Column("nombre_concepto", TypeName = "nvarchar(100)")]
    public string NombreConcepto { get; set; } = string.Empty;

    // opcional si lo mantienes por compatibilidad
    [Column("tipo_concepto")]
    public bool TipoConcepto { get; set; }

    // NUEVO: código único legible (HABER_BASICO, RC_IVA_13, etc.)
    [Required, StringLength(50)]
    [Column("codigo", TypeName = "nvarchar(50)")]
    public string Codigo { get; set; } = string.Empty;

    // NUEVO: FK a Clasificador
    [Required]
    [Column("id_naturaleza")]
    public int IdNaturaleza { get; set; }   // NATURALEZA_CONCEPTO: INGRESO, DESCUENTO_TRAB, APORTE_PATRONAL, ...

    [Required]
    [Column("id_metodo_calculo")]
    public int IdMetodoCalculo { get; set; }  // METODO_CALCULO: FIJO, PORCENTAJE, FORMULA

    // Parámetros posibles del cálculo
    [Column("porcentaje", TypeName = "decimal(9,6)")]
    public decimal? Porcentaje { get; set; }   // si PORCENTAJE (0.13 = 13%)

    [Column("monto", TypeName = "decimal(18,2)")]
    public decimal? Monto { get; set; }        // si FIJO

    // Flags de aplicación
    [Column("aplica_mensual")]  public bool AplicaMensual  { get; set; }
    [Column("aplica_aportes")]  public bool AplicaAportes  { get; set; }
    [Column("aplica_aguinaldo")]public bool AplicaAguinaldo{ get; set; }
    [Column("aplica_prima")]    public bool AplicaPrima    { get; set; }

    // Orden de cómputo y visibilidad
    [Required]
    [Column("orden_calculo")]
    public int OrdenCalculo { get; set; }

    [Column("es_visible", TypeName = "bit")]
    public bool EsVisibleEnReporte { get; set; }

    [Column("es_editable", TypeName = "bit")]
    public bool EditablePorUsuario { get; set; }   // true para RC-IVA, etc.

    // Navs
    [ForeignKey(nameof(IdNaturaleza))]
    [InverseProperty(nameof(Clasificador.NaturalezasConcepto))]
    public Clasificador Naturaleza { get; set; } = null!;

    [ForeignKey(nameof(IdMetodoCalculo))]
    [InverseProperty(nameof(Clasificador.MetodosCalculoConcepto))]
    public Clasificador MetodoCalculo { get; set; } = null!;

    public ICollection<TrabajadorPlanillaValor> TrabajadorPlanillaValores { get; set; } = new List<TrabajadorPlanillaValor>();
}

