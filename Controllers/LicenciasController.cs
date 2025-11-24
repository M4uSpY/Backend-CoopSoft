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

        var trabajador = await _db.Trabajadores
            .Include(t => t.Persona)
            .Include(t => t.Horarios)
            .FirstOrDefaultAsync(t => t.IdTrabajador == dto.IdTrabajador);

        if (trabajador is null)
            return BadRequest("El trabajador indicado no existe.");

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

        // copiamos para poder ajustarlos según el tipo
        var fechaInicio = dto.FechaInicio.Date;
        var fechaFin = dto.FechaFin.Date;
        var horaInicio = dto.HoraInicio;
        var horaFin = dto.HoraFin;

        string nombreTipo = tipoLicencia.ValorCategoria;

        // ======================
        //   REGLAS POR TIPO
        // ======================
        switch (nombreTipo)
        {
            case "Paternidad":
                {
                    // Validamos que el rango de fechas tenga exactamente 3 días corridos
                    var dias = (fechaFin.Date - fechaInicio.Date).TotalDays + 1; // incluyente
                    if (dias != 3)
                    {
                        return BadRequest(
                            "La licencia por paternidad debe abarcar exactamente 3 días corridos. " +
                            "Por favor selecciona un rango de 3 días.");
                    }

                    // Ajustamos horas a jornada completa en ese rango
                    AjustarHorasJornadaCompleta(horarios, fechaInicio, fechaFin, out horaInicio, out horaFin);
                    break;
                }

            case "Matrimonio":
            case "Luto / Duelo":
                // 3 jornadas laborales (dejamos que el helper cuente las que correspondan)
                // Usamos un rango de varios días, pero luego validamos que no supere 3 jornadas.
                AjustarHorasJornadaCompleta(horarios, fechaInicio, fechaFin, out horaInicio, out horaFin);
                break;

            case "Examen Papanicolaou / Mamografía":
            case "Examen Próstata":
            case "Examen Colon":
                // 1 día completo
                fechaFin = fechaInicio;
                AjustarHorasJornadaCompleta(horarios, fechaInicio, fechaFin, out horaInicio, out horaFin);
                break;

            case "Cumpleaños":
                {
                    var fn = trabajador.Persona.FechaNacimiento;
                    var thisYearBirth = new DateTime(DateTime.Today.Year, fn.Month, fn.Day);

                    // Si ya pasó el cumpleaños de este año → no puede pedir
                    if (DateTime.Today > thisYearBirth)
                    {
                        return BadRequest(
                            $"La licencia por cumpleaños sólo puede solicitarse hasta el día del cumpleaños " +
                            $"({thisYearBirth:dd/MM/yyyy}).");
                    }

                    // Sólo se permite el día del cumpleaños
                    fechaInicio = fechaFin = thisYearBirth;

                    var horarioCumple = ObtenerHorarioDia(horarios, thisYearBirth);
                    if (horarioCumple is null)
                        return BadRequest("El trabajador no tiene horario definido el día de su cumpleaños.");

                    var duracion = horarioCumple.HoraSalida - horarioCumple.HoraEntrada;
                    var mitad = TimeSpan.FromMinutes(duracion.TotalMinutes / 2);

                    // Media jornada
                    horaInicio = horarioCumple.HoraEntrada;
                    horaFin = horarioCumple.HoraEntrada + mitad;

                    // Opcional: evitar que pida más de una licencia por cumple en el mismo año
                    bool yaPidioCumple = await _db.Licencias.AnyAsync(l =>
                        l.IdTrabajador == dto.IdTrabajador &&
                        l.IdTipoLicencia == tipoLicencia.IdClasificador &&
                        l.FechaInicio.Year == thisYearBirth.Year);

                    if (yaPidioCumple)
                        return BadRequest("Ya se registró una licencia por cumpleaños para este año.");
                }
                break;

            case "Capacitación / Formación profesional":
                {
                    var horasDia = (horaFin - horaInicio).TotalHours;
                    if (horasDia > 2.1)
                        return BadRequest("La licencia por capacitación no puede exceder 2 horas diarias.");
                }
                break;

            case "Permiso temporal":
                // acá dejamos fechas/horas como el usuario elija,
                // pero luego validamos las 3 horas mensuales.
                break;

            case "Maternidad":
                // No tocamos fechas: puede ser prenatal o postnatal.
                // Sólo validamos que no supere 45 jornadas después del cálculo.
                break;

            case "Estado crítico de salud de hijos":
                // D.S. 3462 prevé varios escenarios; simplificamos a un máximo
                // de 30 días/jornadas laborales continuas o discontinuas.
                break;
        }

        if (fechaFin < fechaInicio)
            return BadRequest("La fecha fin no puede ser menor a la fecha inicio.");

        if (horaFin <= horaInicio)
            return BadRequest("La hora fin debe ser mayor que la hora inicio.");

        // Duración en horas (útil para permisos)
        var horasSolicitudActual = (horaFin - horaInicio).TotalHours;
        if (horasSolicitudActual <= 0)
            return BadRequest("La duración de la licencia debe ser mayor a 0 horas.");

        // ===========================
        //  REGLA ESPECIAL PERMISO TEMPORAL (3h/mes)
        // ===========================
        if (nombreTipo == "Permiso temporal")
        {
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


        // 🔽 AQUÍ VA LA NUEVA REGLA DE NO SOLAPAR 🔽

        // ===========================
        //  EVITAR SOLAPAMIENTO DE LICENCIAS
        // ===========================

        // Buscamos licencias (no rechazadas) que se crucen en FECHAS
        var licenciasSolapadas = await _db.Licencias
            .Include(l => l.EstadoLicencia)
            .Include(l => l.TipoLicencia)
            .Where(l =>
                l.IdTrabajador == dto.IdTrabajador &&
                l.EstadoLicencia.ValorCategoria != "Rechazado" &&
                // RANGO DE FECHAS SOLAPADO:
                l.FechaInicio <= fechaFin &&
                l.FechaFin >= fechaInicio)
            .ToListAsync();

        if (licenciasSolapadas.Any())
        {
            var existente = licenciasSolapadas.First();

            return BadRequest(
                $"El trabajador ya tiene una licencia registrada entre " +
                $"{existente.FechaInicio:dd/MM/yyyy} y {existente.FechaFin:dd/MM/yyyy} " +
                $"(tipo: {existente.TipoLicencia.ValorCategoria}). " +
                "No se permiten licencias que se solapen en fechas.");
        }

        // 🔼 NUEVA REGLA AQUÍ 🔼

        // ===========================
        //  CÁLCULO DE JORNADAS
        // ===========================
        var cantidadJornadas = LicenciaHelper.CalcularCantidadJornadas(
            new LicenciaCrearDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                HoraInicio = horaInicio,
                HoraFin = horaFin
            },
            horarios);
        // ===========================
        //  TOPES DE JORNADAS POR TIPO
        // ===========================
        decimal maxJornadas = 0m;
        switch (nombreTipo)
        {
            case "Maternidad":
                maxJornadas = 45m;
                break;

            case "Paternidad":
            case "Matrimonio":
            case "Luto / Duelo":
                maxJornadas = 3m;
                break;

            case "Cumpleaños":
                maxJornadas = 0.5m; // media jornada
                break;

            case "Estado crítico de salud de hijos":
                maxJornadas = 30m; // simplificado
                break;
        }


        if (maxJornadas > 0m && cantidadJornadas > maxJornadas)
        {
            return BadRequest(
                $"La licencia de tipo '{nombreTipo}' no puede exceder {maxJornadas} jornadas laborales. " +
                $"La solicitud actual equivale a {cantidadJornadas} jornadas.");
        }

        // VALIDACIÓN DE EXACTAMENTE 3 JORNADAS PARA CIERTOS TIPOS
        if (nombreTipo == "Paternidad" || nombreTipo == "Matrimonio" || nombreTipo == "Luto / Duelo")
        {
            if (cantidadJornadas < 3m)
            {
                return BadRequest(
                    $"La licencia de tipo '{nombreTipo}' debe ser exactamente de 3 jornadas laborales. " +
                    $"La solicitud actual equivale a {cantidadJornadas} jornadas.");
            }
        }
        // ===========================
        //  EVITAR DUPLICAR LICENCIA IGUAL
        //  (mismo trabajador, mismo tipo, mismo rango de fechas/horas)
        // ===========================
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
