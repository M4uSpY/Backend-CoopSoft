using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.VacacionesPermisos;
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
    }
}
