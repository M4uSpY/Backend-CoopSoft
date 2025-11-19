using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.VacacionesPermisos;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("SolicitudesCalendario")]
        public async Task<IActionResult> ObtenerSolicitudesCalendario()
        {
            var solicitudes = await _db.Solicitudes.Include(s => s.Trabajador).ThenInclude(t => t.Persona).Include(s => s.TipoSolicitud).Include(s => s.EstadoSolicitud).Select(s => new SolicitudCalendarioDTO
            {
                IdSolicitud = s.IdSolicitud,
                Trabajador = s.Trabajador.Persona.PrimerNombre + " " + s.Trabajador.Persona.ApellidoPaterno,
                TipoSolicitud = s.TipoSolicitud.ValorCategoria,
                EstadoSolicitud = s.EstadoSolicitud.ValorCategoria,

                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin
            }).ToListAsync();

            return Ok(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitudes()
        {
            var solicitudes = await _db.Solicitudes.Include(s => s.Trabajador).ThenInclude(t => t.Persona).Include(s => s.TipoSolicitud).Include(s => s.EstadoSolicitud).Select(s => new SolicitudVacPermListarDTO
            {
                IdSolicitud = s.IdSolicitud,
                CI = s.Trabajador.Persona.CarnetIdentidad,
                ApellidosNombres = s.Trabajador.Persona.ApellidoPaterno + s.Trabajador.Persona.ApellidoMaterno + s.Trabajador.Persona.SegundoNombre + s.Trabajador.Persona.PrimerNombre,
                Cargo = s.Trabajador.Cargo.NombreCargo,
                Tipo = s.TipoSolicitud.ValorCategoria,
                Motivo = s.Motivo,
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                Estado = s.EstadoSolicitud.ValorCategoria
            }).ToListAsync();

            return Ok(solicitudes);
        }

        [HttpPut("{id:int}/aprobar")]
        public async Task<IActionResult> AprobarSolicitud(int id)
        {
            var solicitud = await _db.Solicitudes
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud is null)
                return NotFound();

            // Buscar id de "Aprobado" en Clasificador (EstadoSolicitud)
            var idAprobado = await _db.Clasificadores
                .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Aprobado")
                .Select(c => c.IdClasificador)
                .FirstAsync();

            solicitud.IdEstadoSolicitud = idAprobado;
            solicitud.FechaAprobacion = DateTime.Today;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}/rechazar")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            var solicitud = await _db.Solicitudes
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud is null)
                return NotFound();

            // Buscar id de "Rechazado" en Clasificador (EstadoSolicitud)
            var idRechazado = await _db.Clasificadores
                .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Rechazado")
                .Select(c => c.IdClasificador)
                .FirstAsync();

            solicitud.IdEstadoSolicitud = idRechazado;
            solicitud.FechaAprobacion = DateTime.Today;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudVacPermCrearDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que el trabajador exista
            var trabajadorExiste = await _db.Trabajadores
                .AnyAsync(t => t.IdTrabajador == dto.IdTrabajador);

            if (!trabajadorExiste)
                return BadRequest("El trabajador indicado no existe.");

            // Validar tipo de solicitud (Clasificador)
            var tipoSolicitud = await _db.Clasificadores
                .FirstOrDefaultAsync(c =>
                    c.IdClasificador == dto.IdTipoSolicitud &&
                    c.Categoria == "TipoSolicitud");

            if (tipoSolicitud is null)
                return BadRequest("El tipo de solicitud no es válido.");

            // Obtener el estado "Pendiente"
            var estadoPendiente = await _db.Clasificadores
                .FirstOrDefaultAsync(c =>
                    c.Categoria == "EstadoSolicitud" &&
                    c.ValorCategoria == "Pendiente");

            if (estadoPendiente is null)
                return StatusCode(500, "No está configurado el estado 'Pendiente' en Clasificador.");

            // Crear entidad Solicitud
            var solicitud = new Solicitud
            {
                IdTrabajador = dto.IdTrabajador,
                IdTipoSolicitud = tipoSolicitud.IdClasificador,
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

            // puedes devolver solo el id o un DTO resumido
            return CreatedAtAction(nameof(ObtenerSolicitudes), new { id = solicitud.IdSolicitud }, solicitud.IdSolicitud);
        }
    }
}
