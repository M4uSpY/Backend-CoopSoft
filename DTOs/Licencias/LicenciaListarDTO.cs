using System;

namespace BackendCoopSoft.DTOs.Licencias;

public class LicenciaListarDTO
{
    public int IdLicencia { get; set; }
    public int IdTrabajador { get; set; }

    public string CI { get; set; } = string.Empty;
    public string ApellidosNombres { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;

    public string TipoLicencia { get; set; } = string.Empty; // texto de Clasificador
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }

    public decimal CantidadJornadas { get; set; }

     public string Estado { get; set; } = string.Empty;
     
    public string Motivo { get; set; } = string.Empty;
    public string? Observacion { get; set; }

    public bool TieneArchivoJustificativo { get; set; }
}