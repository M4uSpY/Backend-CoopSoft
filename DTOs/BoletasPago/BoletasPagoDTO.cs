using System;

namespace BackendCoopSoft.DTOs.BoletasPago;

public class BoletasPagoDTO
{
    public int IdTrabajador { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string Mes { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string? Lugar { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Monto { get; set; }
    public byte[] ArchivoPDF { get; set; } = Array.Empty<byte>();
}
