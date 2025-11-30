using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Clasificador")]
public class Clasificador
{
    [Key]
    [Column("id_clasificador")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdClasificador { get; set; }

    [Required(ErrorMessage = "El nombre de la categoria es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de la categoria no puede superar los 50 caracteres")]
    [Column("categoria", TypeName = "nvarchar(50)")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "El valor de la categoria es obligatorio.")]
    [StringLength(50, ErrorMessage = "El valor no puede superar los 50 caracteres")]
    [Column("valor_categoria", TypeName = "nvarchar(50)")]
    public string ValorCategoria { get; set; } = string.Empty;

    [Column("descripcion", TypeName = "nvarchar(max)")]
    public string? Descripcion { get; set; }


    // Relacion 1 Clasif -> N Estado_solicitud
    [InverseProperty(nameof(Vacacion.EstadoSolicitud))]
    public ICollection<Vacacion> EstadosSolicitud { get; set; } = new List<Vacacion>();

    [InverseProperty(nameof(Licencia.EstadoLicencia))]
    public ICollection<Licencia> EstadosLicencia { get; set; } = new List<Licencia>();

    // 1:N Persona
    public ICollection<Persona> Personas { get; set; } = new List<Persona>();

    public virtual ICollection<Licencia> Licencias { get; set; } = new List<Licencia>();



    // 1:N Faltas
    public ICollection<Falta> Faltas { get; set; } = new List<Falta>();

    // Relación 1 Clasificador -> N Contrato (tipo de contrato)
    [InverseProperty(nameof(Contrato.TipoContrato))]
    public ICollection<Contrato> TiposContrato { get; set; } = new List<Contrato>();

    // Relación 1 Clasificador -> N Contrato (periodo de pago)
    [InverseProperty(nameof(Contrato.PeriodoPago))]
    public ICollection<Contrato> PeriodosPago { get; set; } = new List<Contrato>();

    // NUEVAS colecciones para las FKs agregadas
    [InverseProperty(nameof(Planilla.TipoPlanilla))]
    public ICollection<Planilla> TiposPlanilla { get; set; } = new List<Planilla>();

    [InverseProperty(nameof(Concepto.Naturaleza))]
    public ICollection<Concepto> NaturalezasConcepto { get; set; } = new List<Concepto>();

    [InverseProperty(nameof(Concepto.MetodoCalculo))]
    public ICollection<Concepto> MetodosCalculoConcepto { get; set; } = new List<Concepto>();



}

