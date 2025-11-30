using System.Globalization;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Licencias;
using BackendCoopSoft.Models;
using BackendCoopSoft.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LicenciasController : ControllerBase
{
    private readonly AppDbContext _db;
    private const int ID_TIPO_LICENCIA_PERMISO_TEMPORAL = 25; // <-- CAMBIA ESTO

    public LicenciasController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerLicencias()
    {
        var licencias = await _db.Licencias
            .Include(l => l.Trabajador).ThenInclude(t => t.Persona)
            .Include(l => l.Trabajador).ThenInclude(t => t.Cargo)
            .Include(l => l.TipoLicencia)
            .Include(l => l.EstadoLicencia)
            .ToListAsync();

        var dto = licencias.Select(l => new LicenciaListarDTO
        {
            IdLicencia = l.IdLicencia,
            IdTrabajador = l.IdTrabajador,
            CI = l.Trabajador.Persona.CarnetIdentidad,
            ApellidosNombres = $"{l.Trabajador.Persona.ApellidoPaterno} {l.Trabajador.Persona.ApellidoMaterno} {l.Trabajador.Persona.PrimerNombre}",
            Cargo = l.Trabajador.Cargo.NombreCargo,
            TipoLicencia = l.TipoLicencia.ValorCategoria,
            FechaInicio = l.FechaInicio,
            FechaFin = l.FechaFin,
            HoraInicio = l.HoraInicio,
            HoraFin = l.HoraFin,
            CantidadJornadas = l.CantidadJornadas,
            Estado = l.EstadoLicencia.ValorCategoria,
            Motivo = l.Motivo,
            Observacion = l.Observacion,
            TieneArchivoJustificativo = l.ArchivoJustificativo != null && l.ArchivoJustificativo.Length > 0
        }).ToList();

        return Ok(dto);
    }

    [HttpGet("trabajador/{idTrabajador:int}")]
    public async Task<IActionResult> ObtenerLicenciasPorTrabajador(int idTrabajador)
    {
        var licencias = await _db.Licencias
            .Include(l => l.TipoLicencia)
            .Include(l => l.EstadoLicencia)
            .Where(l => l.IdTrabajador == idTrabajador)
            .OrderByDescending(l => l.FechaInicio)
            .ToListAsync();

        var dto = licencias.Select(l => new LicenciaListarDTO
        {
            IdLicencia = l.IdLicencia,
            IdTrabajador = l.IdTrabajador,
            TipoLicencia = l.TipoLicencia.ValorCategoria,
            FechaInicio = l.FechaInicio,
            FechaFin = l.FechaFin,
            HoraInicio = l.HoraInicio,
            HoraFin = l.HoraFin,
            CantidadJornadas = l.CantidadJornadas,
            Estado = l.EstadoLicencia.ValorCategoria,
            Motivo = l.Motivo,
            Observacion = l.Observacion,
            TieneArchivoJustificativo = l.ArchivoJustificativo != null && l.ArchivoJustificativo.Length > 0
        }).ToList();

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CrearLicencia([FromBody] LicenciaCrearDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1) Trabajador
        var trabajador = await _db.Trabajadores
            .Include(t => t.Persona)
            .Include(t => t.Horarios)
            .FirstOrDefaultAsync(t => t.IdTrabajador == dto.IdTrabajador);

        if (trabajador is null)
            return BadRequest("El trabajador indicado no existe.");

        // 2) Tipo de licencia
        var tipoLicencia = await _db.Clasificadores
            .FirstOrDefaultAsync(c =>
                c.IdClasificador == dto.IdTipoLicencia &&
                c.Categoria == "TipoLicencia");

        if (tipoLicencia is null)
            return BadRequest("El tipo de licencia no es válido.");

        var estadoPendiente = await _db.Clasificadores
            .FirstOrDefaultAsync(c =>
                c.Categoria == "EstadoSolicitud" &&
                c.ValorCategoria == "Pendiente");

        if (estadoPendiente is null)
            return StatusCode(500, "No está configurado el estado 'Pendiente'.");

        var horarios = trabajador.Horarios.ToList();

        // Copias locales que vamos a ajustar según reglas
        var fechaInicio = dto.FechaInicio.Date;
        var fechaFin = dto.FechaFin.Date;
        var horaInicio = dto.HoraInicio;
        var horaFin = dto.HoraFin;

        if (fechaFin < fechaInicio)
            return BadRequest("La fecha de fin no puede ser anterior a la fecha de inicio.");

        if (fechaInicio == fechaFin && horaFin <= horaInicio)
            return BadRequest("En licencias de un solo día, la hora de fin debe ser mayor a la hora de inicio.");

        string nombreTipo = (tipoLicencia.ValorCategoria ?? string.Empty).Trim();

        // ⭐ Detectar permiso temporal por nombre (case-insensitive)
        bool esPermisoTemporal =
            string.Equals(nombreTipo, "Permiso temporal", StringComparison.OrdinalIgnoreCase);

        // ======================================================
        //  REGLAS ESPECÍFICAS POR TIPO DE LICENCIA (resumen)
        // ======================================================
        switch (nombreTipo)
        {
            case "Permiso temporal":
                {
                    // ⭐ Debe ser un solo día
                    if (fechaInicio != fechaFin)
                        return BadRequest("El permiso temporal debe solicitarse para un solo día.");

                    // ⭐ Máximo 3 horas por solicitud
                    var duracionHoras = (horaFin - horaInicio).TotalHours;
                    if (duracionHoras <= 0)
                        return BadRequest("La duración del permiso debe ser mayor a cero.");
                    if (duracionHoras > 3.0)
                        return BadRequest("Cada permiso temporal no puede exceder las 3 horas.");
                }
                break;

            case "Paternidad":
                {
                    // Ejemplo: 3 días corridos
                    var dias = (fechaFin - fechaInicio).TotalDays + 1;
                    if (dias != 3)
                        return BadRequest("La licencia por paternidad debe ser de exactamente 3 días consecutivos.");
                }
                break;

            case "Luto / Duelo":
                {
                    // Aquí puedes aplicar la cantidad de días según tu normativa
                    // (Dejo la lógica general, puedes ajustar los días exactos)
                    var dias = (fechaFin - fechaInicio).TotalDays + 1;
                    if (dias <= 0)
                        return BadRequest("La licencia por luto/deudo debe tener al menos un día.");
                }
                break;

            case "Matrimonio":
                {
                    // Igual que arriba, puedes forzar X días si lo necesitas
                    var dias = (fechaFin - fechaInicio).TotalDays + 1;
                    if (dias <= 0)
                        return BadRequest("La licencia por matrimonio debe tener al menos un día.");
                }
                break;

            // 🔹 Otras licencias especiales que tú tenías
            //     (cumpleaños, capacitación, etc.)
            //     las puedes volver a añadir aquí encima del default
            default:
                break;
        }

        // ======================================================
        //  REGLA ESPECIAL: PERMISO TEMPORAL 3h/MES  ⭐⭐⭐
        // ======================================================
        if (esPermisoTemporal)
        {
            var horasSolicitudActual = (horaFin - horaInicio).TotalHours;

            int year = fechaInicio.Year;
            int month = fechaInicio.Month;

            var licenciasMes = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Where(l => l.IdTrabajador == dto.IdTrabajador
                            && l.IdTipoLicencia == tipoLicencia.IdClasificador
                            && l.FechaInicio.Year == year
                            && l.FechaInicio.Month == month
                            && l.EstadoLicencia.ValorCategoria != "Rechazado")
                .ToListAsync();

            double horasYaUsadas = licenciasMes
                .Sum(l => (l.HoraFin - l.HoraInicio).TotalHours);

            if (horasYaUsadas + horasSolicitudActual > 3.0)
            {
                double restantes = 3.0 - horasYaUsadas;
                if (restantes < 0) restantes = 0;

                return BadRequest(
                    $"El trabajador ya utilizó {horasYaUsadas:0.##} h de permiso temporal este mes. " +
                    $"Sólo le quedan {restantes:0.##} h de las 3 h mensuales permitidas.");
            }
        }

        // ======================================================
        //  EVITAR CHOQUE CON VACACIONES / PERMISOS (Solicitudes) ⭐
        // ======================================================
        var haySolicitudAprobada = await _db.Vacaciones
            .Include(s => s.EstadoSolicitud)
            .Where(s => s.IdTrabajador == dto.IdTrabajador
                        && s.EstadoSolicitud.Categoria == "EstadoSolicitud"
                        && s.EstadoSolicitud.ValorCategoria == "Aprobado"
                        && s.FechaInicio <= fechaFin
                        && s.FechaFin >= fechaInicio)
            .AnyAsync();

        if (haySolicitudAprobada)
        {
            return BadRequest("El rango de la licencia se solapa con una vacación o permiso ya aprobado.");
        }

        // ======================================================
        //  EVITAR SOLAPAMIENTO CON OTRAS LICENCIAS             ⭐
        // ======================================================
        var licenciasSolapadas = await _db.Licencias
            .Include(l => l.EstadoLicencia)
            .Include(l => l.TipoLicencia)
            .Where(l =>
                l.IdTrabajador == dto.IdTrabajador &&
                l.EstadoLicencia.ValorCategoria != "Rechazado" &&
                l.FechaInicio <= fechaFin &&
                l.FechaFin >= fechaInicio)
            .ToListAsync();

        if (licenciasSolapadas.Any())
        {
            var existente = licenciasSolapadas.First();
            return BadRequest(
                $"El rango seleccionado se solapa con otra licencia de tipo '{existente.TipoLicencia.ValorCategoria}' " +
                $"del {existente.FechaInicio:dd/MM/yyyy} al {existente.FechaFin:dd/MM/yyyy}.");
        }

        // ======================================================
        //  LIMITAR LICENCIAS ÚNICAS (Luto / Duelo, Paternidad, Matrimonio) ⭐
        // ======================================================
        if (nombreTipo == "Luto / Duelo" ||
            nombreTipo == "Paternidad" ||
            nombreTipo == "Matrimonio")
        {
            bool yaExisteMismaLicencia = await _db.Licencias.AnyAsync(l =>
                l.IdTrabajador == dto.IdTrabajador &&
                l.IdTipoLicencia == tipoLicencia.IdClasificador &&
                l.FechaInicio == fechaInicio &&
                l.FechaFin == fechaFin &&
                l.HoraInicio == horaInicio &&
                l.HoraFin == horaFin);

            if (yaExisteMismaLicencia)
            {
                return BadRequest(
                    $"Ya existe una licencia de tipo '{nombreTipo}' para este trabajador " +
                    $"con el mismo rango de fechas y horas.");
            }
        }

        // ======================================================
        //  CANTIDAD DE JORNADAS (usando tu helper actual)
        // ======================================================
        var cantidadJornadas = LicenciaHelper.CalcularCantidadJornadas(
            new LicenciaCrearDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                HoraInicio = horaInicio,
                HoraFin = horaFin
            },
            horarios);

        // ======================================================
        //  CREAR LICENCIA EN ESTADO PENDIENTE
        // ======================================================
        var licencia = new Licencia
        {
            IdTrabajador = dto.IdTrabajador,
            IdTipoLicencia = tipoLicencia.IdClasificador,
            IdEstadoLicencia = estadoPendiente.IdClasificador,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            CantidadJornadas = cantidadJornadas,
            Motivo = dto.Motivo,
            Observacion = dto.Observacion,
            ArchivoJustificativo = dto.ArchivoJustificativo ?? Array.Empty<byte>(),
            FechaRegistro = DateTime.Now
        };

        _db.Licencias.Add(licencia);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerLicencias), new { id = licencia.IdLicencia }, licencia.IdLicencia);
    }


    [HttpGet("{id:int}/archivo")]
    public async Task<IActionResult> DescargarArchivoJustificativo(int id)
    {
        var licencia = await _db.Licencias
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        if (licencia.ArchivoJustificativo == null || licencia.ArchivoJustificativo.Length == 0)
            return NotFound("La licencia no tiene archivo justificativo.");

        var nombreArchivo = $"Justificativo_Licencia_{id}.pdf";

        return File(
            licencia.ArchivoJustificativo,
            "application/pdf",
            nombreArchivo
        );
    }


    // PUT api/Licencias/5/aprobar
    [HttpPut("{id:int}/aprobar")]
    public async Task<IActionResult> AprobarLicencia(int id)
    {
        var licencia = await _db.Licencias
            .Include(l => l.EstadoLicencia)
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        // Buscar Id de "Aprobado" en Clasificador (EstadoSolicitud)
        var idAprobado = await _db.Clasificadores
            .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Aprobado")
            .Select(c => c.IdClasificador)
            .FirstOrDefaultAsync();

        if (idAprobado == 0)
            return StatusCode(500, "No está configurado el estado 'Aprobado' en Clasificador.");

        licencia.IdEstadoLicencia = idAprobado;
        licencia.FechaAprobacion = DateTime.Today;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT api/Licencias/5/rechazar
    [HttpPut("{id:int}/rechazar")]
    public async Task<IActionResult> RechazarLicencia(int id)
    {
        var licencia = await _db.Licencias
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        // Buscar Id de "Rechazado" en Clasificador (EstadoSolicitud)
        var idRechazado = await _db.Clasificadores
            .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Rechazado")
            .Select(c => c.IdClasificador)
            .FirstOrDefaultAsync();

        if (idRechazado == 0)
            return StatusCode(500, "No está configurado el estado 'Rechazado' en Clasificador.");

        licencia.IdEstadoLicencia = idRechazado;
        licencia.FechaAprobacion = DateTime.Today;

        await _db.SaveChangesAsync();
        return NoContent();
    }


    // helpers internos del controlador
    private static Horario? ObtenerHorarioDia(List<Horario> horarios, DateTime fecha)
    {
        var cultura = new CultureInfo("es-ES");
        string diaSemana = fecha.ToString("dddd", cultura);
        diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);
        return horarios.FirstOrDefault(h => h.DiaSemana == diaSemana);
    }

    private static void AjustarHorasJornadaCompleta(
        List<Horario> horarios,
        DateTime fechaInicio,
        DateTime fechaFin,
        out TimeSpan horaMinima,
        out TimeSpan horaMaxima)
    {
        horaMinima = TimeSpan.MaxValue;
        horaMaxima = TimeSpan.Zero;

        for (var f = fechaInicio.Date; f <= fechaFin.Date; f = f.AddDays(1))
        {
            var h = ObtenerHorarioDia(horarios, f);
            if (h is null) continue;

            if (h.HoraEntrada < horaMinima) horaMinima = h.HoraEntrada;
            if (h.HoraSalida > horaMaxima) horaMaxima = h.HoraSalida;
        }

        if (horaMinima == TimeSpan.MaxValue || horaMaxima == TimeSpan.Zero)
        {
            horaMinima = TimeSpan.FromHours(8);
            horaMaxima = TimeSpan.FromHours(16);
        }
    }
}
