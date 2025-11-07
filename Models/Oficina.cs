using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Oficina")]
public class Oficina
{
    [Key]
    [Column("id_oficina")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdOficina { get; set; }

    [Required(ErrorMessage = "El nombre de la oficina es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre de la oficina no dede superar los 100 caracteres")]
    [Column("nombre", TypeName = "nvarchar(100)")]
    public string Nombre { get; set; } = string.Empty;

    [Column("direccion", TypeName = "nvarchar(max)")]
    public string? Direccion { get; set; }

    [Column("telefono", TypeName = "varchar(8)")]
    public string? Telefono { get; set; } = string.Empty;

    // Relacion (1 Oficina -> N Cargos)
    public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
}
