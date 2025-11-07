using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Rol")]
public class Rol
{
    [Key] 
    [Column("id_rol")] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdRol { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre del rol no puede superar los 50 caracteres.")]
    [Column("nombre_rol", TypeName = "nvarchar(50)")]
    public string NombreRol { get; set; } = string.Empty;

    [Column("descripcion", TypeName = "nvarchar(max)")]
    public string? Descripcion { get; set; }

    // Relacion 1 Rol -> N Usuarios
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
