using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.VacacionesPermisos;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class VacacionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private const string CARGO_ADMIN = "Administrador General";

        public VacacionesController(AppDbContext db)
        {
            _db = db;
        }

        private string? GetRolUsuarioActual()
        {
            // Tu JWT usa ClaimTypes.Role
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        private static bool EsCargoAdministrador(string? nombreCargo)
        {
            if (string.IsNullOrWhiteSpace(nombreCargo))
                return false;

            return string.Equals(nombreCargo.Trim(), CARGO_ADMIN, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsCargoCasual(string? nombreCargo)
        {
            // En tu caso: cualquier cargo que NO sea "Administrador General"
            return !EsCargoAdministrador(nombreCargo);
        }

        // nombreCargoTrabajador = trabajador.Cargo.NombreCargo
        private static bool PuedeGestionarSegunRol(string? rolActual, string? nombreCargoTrabajador)
        {
            if (string.IsNullOrWhiteSpace(rolActual))
                return false;

            rolActual = rolActual.Trim();

            // 👉 Consejo: solo tramita solicitudes de trabajadores con cargo "Administrador General"
            if (string.Equals(rolActual, "Consejo", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rolActual, "Consejo de Administración", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rolActual, "Consejo de Administracion", StringComparison.OrdinalIgnoreCase))
            {
                return EsCargoAdministrador(nombreCargoTrabajador);
            }

            // 👉 Administrador: solo tramita solicitudes de trabajadores "casuales"
            if (string.Equals(rolActual, "Administrador", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rolActual, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return EsCargoCasual(nombreCargoTrabajador);
            }

            // Otros roles no deberían gestionar nada
            return false;
        }

        // ===================== CALENDARIO (SOLO VACACIONES) =====================
        [HttpGet("SolicitudesCalendario")]
        public async Task<IActionResult> ObtenerSolicitudesCalendario()
        {
            var solicitudesVacaciones = await _db.Vacaciones
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(s => s.EstadoSolicitud)
                .Select(s => new SolicitudCalendarioDTO
                {
                    IdVacacion = s.IdVacacion,
                    Trabajador = s.Trabajador.Persona.PrimerNombre + " " + s.Trabajador.Persona.ApellidoPaterno,
                    TipoSolicitud = "Vacación",
                    EstadoSolicitud = s.EstadoSolicitud.ValorCategoria,
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin
                })
                .ToListAsync();

            return Ok(solicitudesVacaciones);
        }

        // ==================== LISTA PARA ADMIN/CONSEJO =====================
        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitudes()
        {
            var rolActual = GetRolUsuarioActual();
            if (string.IsNullOrWhiteSpace(rolActual))
                return Forbid("No se pudo determinar el rol del usuario actual.");

            var rol = rolActual.Trim();

            var query = _db.Vacaciones
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(s => s.EstadoSolicitud)
                .AsQueryable();

            // Consejo → solo "Administrador General"
            if (rol.Equals("Consejo", StringComparison.OrdinalIgnoreCase) ||
                rol.Equals("Consejo de Administración", StringComparison.OrdinalIgnoreCase) ||
                rol.Equals("Consejo de Administracion", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Trabajador.Cargo.NombreCargo == CARGO_ADMIN);
            }
            // Administrador → solo casuales (no "Administrador General")
            else if (rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                     rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Trabajador.Cargo.NombreCargo != CARGO_ADMIN);
            }
            else
            {
                return Forbid("No tiene permisos para ver la lista de solicitudes de vacación.");
            }

            var solicitudesVacaciones = await query
                .Select(s => new SolicitudVacListarDTO
                {
                    IdVacacion = s.IdVacacion,
                    IdTrabajador = s.IdTrabajador,

                    CI = s.Trabajador.Persona.CarnetIdentidad,
                    ApellidosNombres =
                        s.Trabajador.Persona.ApellidoPaterno + " " +
                        s.Trabajador.Persona.ApellidoMaterno + " " +
                        s.Trabajador.Persona.PrimerNombre,
                    Cargo = s.Trabajador.Cargo.NombreCargo,
                    Tipo = "Vacación",
                    Motivo = s.Motivo,
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin,
                    Estado = s.EstadoSolicitud.ValorCategoria
                })
                .ToListAsync();

            return Ok(solicitudesVacaciones);
        }


        // ==================== APROBAR VACACIÓN =====================
        [HttpPut("{id:int}/aprobar")]
        public async Task<IActionResult> AprobarSolicitud(int id)
        {
            // 1) Rol del usuario actual
            var rolActual = GetRolUsuarioActual();
            if (string.IsNullOrWhiteSpace(rolActual))
                return Forbid("No se pudo determinar el rol del usuario actual.");

            var solicitudesVacacion = await _db.Vacaciones
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .FirstOrDefaultAsync(s => s.IdVacacion == id);

            if (solicitudesVacacion is null)
                return NotFound();

            var cargoTrabajador = solicitudesVacacion.Trabajador?.Cargo?.NombreCargo;

            // 2) Validar permiso según rol y cargo
            if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
                return Forbid("No tiene permiso para aprobar la vacación de este trabajador.");

            // 3) Lógica de negocio de vacaciones (lo que ya tenías)
            bool descuentaVacacion = true;

            if (descuentaVacacion)
            {
                if (solicitudesVacacion.Trabajador is null)
                    return StatusCode(500, "El trabajador no tiene registrada la fecha de ingreso.");

                var fechaIngreso = solicitudesVacacion.Trabajador.FechaIngreso;
                var fechaRef = solicitudesVacacion.FechaInicio.Date;

                var antiguedadAnios = CalcularAntiguedadEnAnios(fechaIngreso, fechaRef);
                var diasDerecho = ObtenerDiasVacacionPorAntiguedad(antiguedadAnios);

                if (diasDerecho == 0)
                    return BadRequest("El trabajador aún no cumple un año de servicio, por lo que no tiene derecho a vacación.");

                var gestion = fechaRef.Year;
                var diasYaUsados = await CalcularDiasVacacionUsadosAsync(
                    solicitudesVacacion.IdTrabajador,
                    gestion,
                    solicitudesVacacion.IdVacacion);

                var diasSolicitudActual = ContarDiasHabiles(
                    solicitudesVacacion.FechaInicio,
                    solicitudesVacacion.FechaFin);

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

            solicitudesVacacion.IdEstadoSolicitud = idAprobado;
            solicitudesVacacion.FechaAprobacion = DateTime.Today;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ==================== RECHAZAR VACACIÓN =====================
        [HttpPut("{id:int}/rechazar")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            // 1) Rol del usuario actual
            var rolActual = GetRolUsuarioActual();
            if (string.IsNullOrWhiteSpace(rolActual))
                return Forbid("No se pudo determinar el rol del usuario actual.");

            var solicitudVacacion = await _db.Vacaciones
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .FirstOrDefaultAsync(s => s.IdVacacion == id);

            if (solicitudVacacion is null)
                return NotFound();

            var cargoTrabajador = solicitudVacacion.Trabajador?.Cargo?.NombreCargo;

            // 2) Validar permiso
            if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
                return Forbid("No tiene permiso para rechazar la vacación de este trabajador.");

            // 3) Lógica normal de rechazo
            var idRechazado = await _db.Clasificadores
                .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Rechazado")
                .Select(c => c.IdClasificador)
                .FirstAsync();

            solicitudVacacion.IdEstadoSolicitud = idRechazado;
            solicitudVacacion.FechaAprobacion = DateTime.Today;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ==================== CREAR SOLICITUD VACACIÓN =====================
        [HttpPost]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudVacCrearDTO dto)
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

            var fechaInicio = dto.FechaInicio.Date;
            var fechaFin = dto.FechaFin.Date;

            // 1) solapamiento con otras vacaciones
            var vacacionesSolapadas = await _db.Vacaciones
                .Include(s => s.EstadoSolicitud)
                .Where(s =>
                    s.IdTrabajador == dto.IdTrabajador &&
                    s.EstadoSolicitud.ValorCategoria != "Rechazado" &&
                    s.FechaInicio <= fechaFin &&
                    s.FechaFin >= fechaInicio)
                .ToListAsync();

            if (vacacionesSolapadas.Any())
            {
                var otra = vacacionesSolapadas.First();

                return BadRequest(
                    $"El trabajador ya tiene una solicitud de vacación registrada entre " +
                    $"{otra.FechaInicio:dd/MM/yyyy} y {otra.FechaFin:dd/MM/yyyy} " +
                    $"(estado: {otra.EstadoSolicitud.ValorCategoria}). " +
                    $"No se permiten vacaciones que se solapen en fechas.");
            }

            // 2) solapamiento con licencias
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
                var lic = licenciasSolapadas.First();

                return BadRequest(
                    $"El trabajador tiene una licencia registrada entre " +
                    $"{lic.FechaInicio:dd/MM/yyyy} y {lic.FechaFin:dd/MM/yyyy} " +
                    $"(tipo: {lic.TipoLicencia.ValorCategoria}, estado: {lic.EstadoLicencia.ValorCategoria}). " +
                    "No se puede solicitar vacación sobre días cubiertos por licencias.");
            }

            var estadoPendiente = await _db.Clasificadores
                .FirstOrDefaultAsync(c =>
                    c.Categoria == "EstadoSolicitud" &&
                    c.ValorCategoria == "Pendiente");

            if (estadoPendiente is null)
                return StatusCode(500, "No está configurado el estado 'Pendiente' en Clasificador.");

            var solicitudVacacion = new Vacacion
            {
                IdTrabajador = dto.IdTrabajador,
                IdEstadoSolicitud = estadoPendiente.IdClasificador,
                Motivo = dto.Motivo,
                Observacion = dto.Observacion,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                FechaSolicitud = DateTime.Today,
                FechaAprobacion = null
            };

            _db.Vacaciones.Add(solicitudVacacion);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerSolicitudes),
                new { id = solicitudVacacion.IdVacacion },
                solicitudVacacion.IdVacacion);
        }
        [HttpGet("ping")]
        public string PingVacaciones()
        {
            return "Vacaciones v2";
        }


        // ==================== EDITAR VACACIÓN =====================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarSolicitud(int id, [FromBody] SolicitudVacEditarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.FechaFin.Date < dto.FechaInicio.Date)
                return BadRequest("La fecha fin no puede ser menor a la fecha inicio.");

            var solicitud = await _db.Vacaciones
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(s => s.EstadoSolicitud)
                .FirstOrDefaultAsync(s => s.IdVacacion == id);

            if (solicitud is null)
                return NotFound("Solicitud de vacación no encontrada.");

            // 1) Validar rol que edita
            var rolActual = GetRolUsuarioActual();
            if (string.IsNullOrWhiteSpace(rolActual))
                return Forbid("No se pudo determinar el rol del usuario actual.");

            var cargoTrabajador = solicitud.Trabajador?.Cargo?.NombreCargo;

            if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
                return Forbid("No tiene permiso para editar la vacación de este trabajador.");

            // 2) Estado permitido para editar: Pendiente o Aprobado
            var estado = solicitud.EstadoSolicitud?.ValorCategoria ?? string.Empty;

            if (!string.Equals(estado, "Pendiente", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(estado, "Aprobado", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Solo se pueden editar solicitudes en estado 'Pendiente' o 'Aprobado'.");
            }

            var nuevaFechaInicio = dto.FechaInicio.Date;
            var nuevaFechaFin = dto.FechaFin.Date;

            // (opcional) Si quieres que solo recorten la vacación:
            // if (nuevaFechaFin > solicitud.FechaFin.Date)
            //     return BadRequest("Solo se permite acortar la vacación, no extenderla.");

            // 3) Validar solapamientos con OTRAS vacaciones
            var vacSolapadas = await _db.Vacaciones
                .Include(v => v.EstadoSolicitud)
                .Where(v =>
                    v.IdTrabajador == solicitud.IdTrabajador &&
                    v.IdVacacion != solicitud.IdVacacion &&              // excluir la que se está editando
                    v.EstadoSolicitud.ValorCategoria != "Rechazado" &&
                    v.FechaInicio <= nuevaFechaFin &&
                    v.FechaFin >= nuevaFechaInicio)
                .ToListAsync();

            if (vacSolapadas.Any())
            {
                var otra = vacSolapadas.First();
                return BadRequest(
                    $"La nueva fecha se solapa con otra vacación del {otra.FechaInicio:dd/MM/yyyy} " +
                    $"al {otra.FechaFin:dd/MM/yyyy} (estado: {otra.EstadoSolicitud.ValorCategoria}).");
            }

            // 4) Validar solapamiento con licencias
            var licSolapadas = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Include(l => l.TipoLicencia)
                .Where(l =>
                    l.IdTrabajador == solicitud.IdTrabajador &&
                    l.EstadoLicencia.ValorCategoria != "Rechazado" &&
                    l.FechaInicio <= nuevaFechaFin &&
                    l.FechaFin >= nuevaFechaInicio)
                .ToListAsync();

            if (licSolapadas.Any())
            {
                var lic = licSolapadas.First();
                return BadRequest(
                    $"La nueva fecha se solapa con una licencia del {lic.FechaInicio:dd/MM/yyyy} " +
                    $"al {lic.FechaFin:dd/MM/yyyy} (tipo: {lic.TipoLicencia.ValorCategoria}, " +
                    $"estado: {lic.EstadoLicencia.ValorCategoria}).");
            }

            // 5) Validar que no se pase del derecho de días (como en aprobar)
            bool descuentaVacacion = true;
            if (descuentaVacacion)
            {
                if (solicitud.Trabajador is null)
                    return StatusCode(500, "El trabajador no tiene registrada la fecha de ingreso.");

                var fechaIngreso = solicitud.Trabajador.FechaIngreso;
                var fechaRef = nuevaFechaInicio;

                var antiguedadAnios = CalcularAntiguedadEnAnios(fechaIngreso, fechaRef);
                var diasDerecho = ObtenerDiasVacacionPorAntiguedad(antiguedadAnios);

                if (diasDerecho == 0)
                    return BadRequest("El trabajador aún no cumple un año de servicio, por lo que no tiene derecho a vacación.");

                var gestion = fechaRef.Year;

                // 👀 Aquí usamos el helper que ya creaste, excluyendo ESTA misma solicitud
                var diasYaUsados = await CalcularDiasVacacionUsadosAsync(
                    solicitud.IdTrabajador,
                    gestion,
                    solicitud.IdVacacion);

                var diasSolicitudNueva = ContarDiasHabiles(nuevaFechaInicio, nuevaFechaFin);

                if (diasYaUsados + diasSolicitudNueva > diasDerecho)
                {
                    var disponible = diasDerecho - diasYaUsados;
                    return BadRequest(
                        $"Con las nuevas fechas, se excede el derecho a vacación. " +
                        $"Días disponibles: {disponible}, días solicitados: {diasSolicitudNueva}.");
                }
            }

            // 6) Aplicar cambios
            solicitud.FechaInicio = nuevaFechaInicio;
            solicitud.FechaFin = nuevaFechaFin;
            solicitud.Motivo = dto.Motivo.Trim();
            solicitud.Observacion = dto.Observacion;

            await _db.SaveChangesAsync();
            return NoContent();
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

        // ==================== ELIMINAR VACACIÓN =====================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarSolicitud(int id)
        {
            // 1) Rol del usuario actual
            var rolActual = GetRolUsuarioActual();
            if (string.IsNullOrWhiteSpace(rolActual))
                return Forbid("No se pudo determinar el rol del usuario actual.");

            var solicitudVacacion = await _db.Vacaciones
                .Include(s => s.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(s => s.EstadoSolicitud)
                .FirstOrDefaultAsync(s => s.IdVacacion == id);

            if (solicitudVacacion is null)
                return NotFound("Solicitud de vacación no encontrada.");

            var cargoTrabajador = solicitudVacacion.Trabajador?.Cargo?.NombreCargo;

            // 2) Validar permiso según rol y cargo
            if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
                return Forbid("No tiene permiso para eliminar la vacación de este trabajador.");

            // 3) Solo permitir eliminar si está en estado 'Pendiente'
            if (!string.Equals(solicitudVacacion.EstadoSolicitud?.ValorCategoria, "Pendiente", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Solo se pueden eliminar solicitudes de vacación en estado 'Pendiente'.");

            _db.Vacaciones.Remove(solicitudVacacion);
            await _db.SaveChangesAsync();

            return NoContent();
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

        private static int ObtenerDiasVacacionPorAntiguedad(int anios)
        {
            if (anios < 1) return 0;
            if (anios < 5) return 15;
            if (anios < 10) return 20;
            return 30;
        }

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

        private async Task<int> CalcularDiasVacacionUsadosAsync(int idTrabajador, int gestion, int idSolicitudActual = 0)
        {
            var solicitudesVacacion = await _db.Vacaciones
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.IdTrabajador == idTrabajador
                            && s.FechaInicio.Year == gestion
                            && s.IdVacacion != idSolicitudActual
                            && s.EstadoSolicitud.ValorCategoria == "Aprobado")
                .ToListAsync();

            int total = 0;

            foreach (var sol in solicitudesVacacion)
            {
                total += ContarDiasHabiles(sol.FechaInicio, sol.FechaFin);
            }

            return total;
        }
    }
}
