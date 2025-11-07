    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCoopSoft.Models;

[Table("Log_Acceso")]
public class LogAcceso
{
    [Key]
    [Column("id_log")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdLog { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("fecha_login", TypeName = "date")]
    public DateTime FechaLogin { get; set; }

    [Column("hora_login", TypeName = "time")]
    public TimeSpan HoraLogin { get; set; }

    [Column("fecha_logout", TypeName = "date")]
    public DateTime? FechaLogout { get; set; }

    [Column("hora_logout", TypeName = "time")]
    public TimeSpan? HoraLogout { get; set; }

    // 🔗 Propiedad de navegación
    [ForeignKey(nameof(IdUsuario))]
    public Usuario Usuario { get; set; } = null!;
}

