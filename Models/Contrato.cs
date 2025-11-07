using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Contrato")]
public class Contrato
{
    [Key]
    [Column("id_contrato")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdContrato { get; set; }

    [Required(ErrorMessage = "El trabajador es requerido")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "El tipo de contrato es requerido")]
    [Column("id_tipo_contrato")]
    public int IdTipoContrato { get; set; }

    [Required(ErrorMessage = "El tipo de periodo es requerido")]
    [Column("id_periodo_pago")]
    public int IdPeriodoPago { get; set; }

    [Required(ErrorMessage = "El nro de contrato es requerido")]
    [Column("numero_contrato", TypeName = "varchar(50)")]
    [StringLength(50)]
    public string NumeroContrato { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    [Column("fecha_inicio", TypeName = "date")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de finalizacion es requerida")]
    [Column("fecha_fin", TypeName = "date")]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El archivo pdf es requerido")]
    [Column("archivo_pdf", TypeName = "varbinary(max)")]
    public byte[] ArchivoPdf { get; set; } = Array.Empty<byte>();

    // FK
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

    [ForeignKey(nameof(IdTipoContrato))]
    [InverseProperty(nameof(Clasificador.TiposContrato))]
    public Clasificador TipoContrato { get; set; } = null!;

    [ForeignKey(nameof(IdPeriodoPago))]
    [InverseProperty(nameof(Clasificador.PeriodosPago))]
    public Clasificador PeriodoPago { get; set; } = null!;
}

