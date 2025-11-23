using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.VacacionesPermisos;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacacionesPermisosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VacacionesPermisosController(AppDbContext db)
        {
            _db = db;
        }

        // ===================== CALENDARIO (SOLO VACACIONES) =====================
        [HttpGet("SolicitudesCalendario")]
        public async Task<IActionResult> ObtenerSolicitudesCalendario()
        {
            var solicitudes = await _db.Solicitudes
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(s => s.EstadoSolicitud)
                .Select(s => new SolicitudCalendarioDTO
                {
                    IdSolicitud = s.IdSolicitud,
                    Trabajador = s.Trabajador.Persona.PrimerNombre + " " + s.Trabajador.Persona.ApellidoPaterno,
                    // Antes estaba s.TipoSolicitud.ValorCategoria
                    // Ahora todas son vacaciones → valor fijo
                    TipoSolicitud = "Vacación",
                    EstadoSolicitud = s.EstadoSolicitud.ValorCategoria,
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin
                })
                .ToListAsync();

            return Ok(solicitudes);
        }

        // ==================== LISTA PARA ADMIN =====================
        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitudes()
        {
            var solicitudes = await _db.Solicitudes
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(s => s.EstadoSolicitud)
                .Select(s => new SolicitudVacPermListarDTO
                {
                    IdSolicitud = s.IdSolicitud,
                    CI = s.Trabajador.Persona.CarnetIdentidad,
                    ApellidosNombres =
                        s.Trabajador.Persona.ApellidoPaterno + " " +
                        s.Trabajador.Persona.ApellidoMaterno + " " +
                        s.Trabajador.Persona.PrimerNombre,
                    Cargo = s.Trabajador.Cargo.NombreCargo,
                    // Antes venía de TipoSolicitud.ValorCategoria
                    Tipo = "Vacación",
                    Motivo = s.Motivo,
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin,
                    Estado = s.EstadoSolicitud.ValorCategoria
                })
                .ToListAsync();

            return Ok(solicitudes);
        }

        // ==================== APROBAR VACACIÓN =====================
        [HttpPut("{id:int}/aprobar")]
        public async Task<IActionResult> AprobarSolicitud(int id)
        {
            var solicitud = await _db.Solicitudes
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Persona)
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud is null)
                return NotFound();

            // Ahora TODAS las solicitudes de esta tabla son VACACIONES
            bool descuentaVacacion = true;

            if (descuentaVacacion)
            {
                if (solicitud.Trabajador is null)
                    return StatusCode(500, "El trabajador no tiene registrada la fecha de ingreso.");

                var fechaIngreso = solicitud.Trabajador.FechaIngreso;
                var fechaRef = solicitud.FechaInicio.Date;

                var antiguedadAnios = CalcularAntiguedadEnAnios(fechaIngreso, fechaRef);
                var diasDerecho = ObtenerDiasVacacionPorAntiguedad(antiguedadAnios);

                if (diasDerecho == 0)
                    return BadRequest("El trabajador aún no cumple un año de servicio, por lo que no tiene derecho a vacación.");

                var gestion = fechaRef.Year;
                var diasYaUsados = await CalcularDiasVacacionUsadosAsync(
                    solicitud.IdTrabajador,
                    gestion,
                    solicitud.IdSolicitud);

                var diasSolicitudActual = ContarDiasHabiles(
                    solicitud.FechaInicio,
                    solicitud.FechaFin);

                if (diasYaUsados + diasSolicitudActual > diasDerecho)
                {
                    var disponible = diasDerecho - diasYaUsados;
                    return BadRequest(
                        $"No se puede aprobar la solicitud. " +
                        $"Días disponibles: {disponible}, días solicitados: {diasSolicitudActual}.");
                }
            }

            var idAprobado = await _db.Clasificadores
                .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Aprobado")
                .Select(c => c.IdClasificador)
                .FirstAsync();

            solicitud.IdEstadoSolicitud = idAprobado;
            solicitud.FechaAprobacion = DateTime.Today;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ==================== RECHAZAR VACACIÓN =====================
        [HttpPut("{id:int}/rechazar")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            var solicitud = await _db.Solicitudes
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud is null)
                return NotFound();

            var idRechazado = await _db.Clasificadores
                .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Rechazado")
                .Select(c => c.IdClasificador)
                .FirstAsync();

            solicitud.IdEstadoSolicitud = idRechazado;
            solicitud.FechaAprobacion = DateTime.Today;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ==================== CREAR SOLICITUD VACACIÓN =====================
        [HttpPost]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudVacPermCrearDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.FechaFin.Date < dto.FechaInicio.Date)
                return BadRequest("La fecha fin no puede ser menor a la fecha inicio.");

            // Validar que el trabajador exista
            var trabajadorExiste = await _db.Trabajadores
                .AnyAsync(t => t.IdTrabajador == dto.IdTrabajador);

            if (!trabajadorExiste)
                return BadRequest("El trabajador indicado no existe.");

            // Estado "Pendiente"
            var estadoPendiente = await _db.Clasificadores
                .FirstOrDefaultAsync(c =>
                    c.Categoria == "EstadoSolicitud" &&
                    c.ValorCategoria == "Pendiente");

            if (estadoPendiente is null)
                return StatusCode(500, "No está configurado el estado 'Pendiente' en Clasificador.");

            var solicitud = new Solicitud
            {
                IdTrabajador = dto.IdTrabajador,
                IdEstadoSolicitud = estadoPendiente.IdClasificador,
                Motivo = dto.Motivo,
                Observacion = dto.Observacion,
                FechaInicio = dto.FechaInicio.Date,
                FechaFin = dto.FechaFin.Date,
                FechaSolicitud = DateTime.Today,
                FechaAprobacion = null
            };

            _db.Solicitudes.Add(solicitud);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerSolicitudes),
                new { id = solicitud.IdSolicitud },
                solicitud.IdSolicitud);
        }

        // ==================== RESUMEN VACACIONES =====================
        [HttpGet("Resumen/{idTrabajador:int}")]
        public async Task<IActionResult> ObtenerResumenVacaciones(int idTrabajador)
        {
            var trabajador = await _db.Trabajadores
                .FirstOrDefaultAsync(t => t.IdTrabajador == idTrabajador);

            if (trabajador is null)
                return NotFound("Trabajador no encontrado.");

            var fechaRef = DateTime.Today;
            var gestion = fechaRef.Year;
            var fechaIngreso = trabajador.FechaIngreso;

            var antiguedadAnios = CalcularAntiguedadEnAnios(fechaIngreso, fechaRef);
            var diasDerecho = ObtenerDiasVacacionPorAntiguedad(antiguedadAnios);

            int diasUsados = 0;

            if (diasDerecho > 0)
            {
                diasUsados = await CalcularDiasVacacionUsadosAsync(idTrabajador, gestion);
                if (diasUsados < 0) diasUsados = 0;
                if (diasUsados > diasDerecho) diasUsados = diasDerecho;
            }

            var dto = new ResumenVacacionesDTO
            {
                Gestion = gestion,
                FechaIngreso = fechaIngreso,
                AntiguedadAnios = antiguedadAnios,
                DiasDerecho = diasDerecho,
                DiasUsados = diasUsados,
                DiasDisponibles = diasDerecho - diasUsados
            };

            return Ok(dto);
        }

        // ==================== HELPERS =====================

        private static int CalcularAntiguedadEnAnios(DateTime fechaIngreso, DateTime fechaReferencia)
        {
            int anios = fechaReferencia.Year - fechaIngreso.Year;

            if (fechaReferencia.Month < fechaIngreso.Month ||
                (fechaReferencia.Month == fechaIngreso.Month && fechaReferencia.Day < fechaIngreso.Day))
            {
                anios--;
            }

            return anios;
        }

        // Días de vacación por ley en Bolivia
        private static int ObtenerDiasVacacionPorAntiguedad(int anios)
        {
            if (anios < 1) return 0;
            if (anios < 5) return 15;
            if (anios < 10) return 20;
            return 30;
        }

        // Cuenta días hábiles (lunes a viernes) entre dos fechas inclusive
        private static int ContarDiasHabiles(DateTime inicio, DateTime fin)
        {
            int dias = 0;
            for (var fecha = inicio.Date; fecha <= fin.Date; fecha = fecha.AddDays(1))
            {
                if (fecha.DayOfWeek != DayOfWeek.Saturday &&
                    fecha.DayOfWeek != DayOfWeek.Sunday)
                {
                    dias++;
                }
            }
            return dias;
        }

        // Días ya usados en el año (ahora SOLO vacaciones, porque esta tabla ya no guarda permisos)
        private async Task<int> CalcularDiasVacacionUsadosAsync(int idTrabajador, int gestion, int idSolicitudActual = 0)
        {
            var solicitudes = await _db.Solicitudes
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.IdTrabajador == idTrabajador
                            && s.FechaInicio.Year == gestion
                            && s.IdSolicitud != idSolicitudActual
                            && s.EstadoSolicitud.ValorCategoria == "Aprobado")
                .ToListAsync();

            int total = 0;

            foreach (var sol in solicitudes)
            {
                total += ContarDiasHabiles(sol.FechaInicio, sol.FechaFin);
            }

            return total;
        }
    }
}
