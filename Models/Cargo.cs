using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Cargo")]
public class Cargo
{
    [Key]
    [Column("id_cargo")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCargo { get; set; }

    [Required(ErrorMessage = "La oficina es obligatoria.")]
    [Column("id_oficina")]
    public int IdOficina { get; set; }

    [Required(ErrorMessage = "El nombre del cargo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del cargo no puede superar los 100 caracteres")]
    [Column("nombre_cargo", TypeName = "nvarchar(100)")]
    public string NombreCargo { get; set; } = string.Empty;

    [Column("descripcion", TypeName = "nvarchar(max)")]
    public string? Descripcion { get; set; }

    // Propiedad de navegacion (para navegar hacia oficina)
    [ForeignKey(nameof(IdOficina))]
    public Oficina Oficina { get; set; } = null!; // no puede ser null

    // 1:N Trabajador
    public ICollection<Trabajador> Trabajadors { get; set; } = new List<Trabajador>();

}
