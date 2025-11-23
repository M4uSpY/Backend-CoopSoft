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

        // copiamos para poder ajustarlos según el tipo
        var fechaInicio = dto.FechaInicio.Date;
        var fechaFin = dto.FechaFin.Date;
        var horaInicio = dto.HoraInicio;
        var horaFin = dto.HoraFin;

        // ======================
        //   REGLAS POR TIPO
        // ======================
        switch (tipoLicencia.ValorCategoria)
        {
            case "Paternidad":
                // 3 días corridos desde la fecha indicada
                fechaFin = fechaInicio.AddDays(2);
                AjustarHorasJornadaCompleta(trabajador.Horarios.ToList(), fechaInicio, fechaFin, out horaInicio, out horaFin);
                break;

            case "Matrimonio":
            case "Luto / Duelo":
                // 3 jornadas laborales → calcular con días hábiles, pero usamos rango y horarios
                fechaFin = fechaInicio.AddDays(6); // luego el helper solo contará los días con horario
                AjustarHorasJornadaCompleta(trabajador.Horarios.ToList(), fechaInicio, fechaFin, out horaInicio, out horaFin);
                break;

            case "Examen Papanicolaou / Mamografía":
            case "Examen Próstata":
            case "Examen Colon":
                // 1 día completo
                fechaFin = fechaInicio;
                AjustarHorasJornadaCompleta(trabajador.Horarios.ToList(), fechaInicio, fechaFin, out horaInicio, out horaFin);
                break;

            case "Cumpleaños":
                var fn = trabajador.Persona.FechaNacimiento;
                var thisYearBirth = new DateTime(DateTime.Today.Year, fn.Month, fn.Day);

                fechaInicio = fechaFin = thisYearBirth;

                var horarioCumple = ObtenerHorarioDia(trabajador.Horarios.ToList(), thisYearBirth);
                if (horarioCumple is null)
                    return BadRequest("El trabajador no tiene horario definido el día de su cumpleaños.");

                var duracion = horarioCumple.HoraSalida - horarioCumple.HoraEntrada;
                var mitad = TimeSpan.FromMinutes(duracion.TotalMinutes / 2);

                horaInicio = horarioCumple.HoraEntrada;
                horaFin = horarioCumple.HoraEntrada + mitad;
                break;

            case "Capacitación / Formación profesional":
                if ((horaFin - horaInicio).TotalHours > 2.1)
                    return BadRequest("La licencia por capacitación no puede exceder 2 horas diarias.");
                break;
        }

        if (fechaFin < fechaInicio)
            return BadRequest("La fecha fin no puede ser menor a la fecha inicio.");

        if (horaFin <= horaInicio)
            return BadRequest("La hora fin debe ser mayor que la hora inicio.");

        var cantidadJornadas = LicenciaHelper.CalcularCantidadJornadas(
            new LicenciaCrearDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                HoraInicio = horaInicio,
                HoraFin = horaFin
            },
            trabajador.Horarios.ToList());

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
            ArchivoJustificativo = Array.Empty<byte>(),
            FechaRegistro = DateTime.Now
        };

        _db.Licencias.Add(licencia);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerLicencias), new { id = licencia.IdLicencia }, licencia.IdLicencia);
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
