using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Huella_Dactilar")]
public class HuellaDactilar
{
    [Key]
    [Column("id_huella")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHuella { get; set; }

    [Required]
    [Column("id_persona")]
    public int IdPersona { get; set; }

    [Required]
    [Column("huella", TypeName = "varbinary(max)")]
    public byte[] Huella { get; set; } = Array.Empty<byte>();

    // 🔗 Propiedad de navegación
    [ForeignKey(nameof(IdPersona))]
    public Persona Persona { get; set; } = null!;
}
