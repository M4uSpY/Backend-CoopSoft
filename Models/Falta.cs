using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Falta")]
public class Falta
{
    [Key]
    [Column("id_falta")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdFalta { get; set; }

    [Required(ErrorMessage = "Es necesario el trabajador")]
    [Column("id_trabajador")]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "Es necesario un tipo de falta")]
    [Column("id_tipo_falta")]
    public int IdTipoFalta { get; set; }

    [Required]
    [Column("fecha", TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Column("descripcion", TypeName = "nvarchar(max)")]
    public string Descripcion { get; set; } = string.Empty;

    [Column("archivo_justificativo", TypeName = "varbinary(max)")]
    public byte[] ArchivoJustificativo { get; set; } = Array.Empty<byte>();

    [Column("estado_falta")]
    public bool EstadoFalta { get; set; } = true;

    // FKs
    [ForeignKey(nameof(IdTrabajador))]
    public Trabajador Trabajador { get; set; } = null!;

    [ForeignKey(nameof(IdTipoFalta))]
    public Clasificador TipoFalta { get; set; } = null!;

    // 1:N hacia el histórico de faltas
    public ICollection<HistoricoFalta> HistoricoFaltas { get; set; } = new List<HistoricoFalta>();
}

