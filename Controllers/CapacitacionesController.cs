using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CapacitacionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public CapacitacionesController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("trabajador/{idTrabajador:int}")]
        public async Task<IActionResult> ObtenerCapacitacionesPorTrabajador(int idTrabajador)
        {
            var capacitaciones = await _db.Capacitaciones
                .Where(c => c.IdTrabajador == idTrabajador)
                .ToListAsync();

            var dto = _mapper.Map<List<CapacitacionResumenDTO>>(capacitaciones);
            return Ok(dto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerCapacitacionPorId(int id)
        {
            var capacitacion = await _db.Capacitaciones
                .FirstOrDefaultAsync(c => c.IdCapacitacion == id);

            if (capacitacion is null)
                return NotFound("Capacitacion no encontrada");

            var dto = _mapper.Map<CapacitacionEditarDTO>(capacitacion);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCapacitacion([FromBody] CapacitacionCrearDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // validar que exista el trabajador
            var existeTrabajador = await _db.Trabajadores
                .AnyAsync(t => t.IdTrabajador == dto.IdTrabajador);

            if (!existeTrabajador)
                return BadRequest("El trabajador no existe");

            var capacitacion = _mapper.Map<Capacitacion>(dto);

            // por seguridad, aseguramos que EF la trate como nueva (IDENTITY)
            capacitacion.IdCapacitacion = 0;

            await _db.Capacitaciones.AddAsync(capacitacion);
            await _db.SaveChangesAsync();

            var resumen = _mapper.Map<CapacitacionResumenDTO>(capacitacion);

            return CreatedAtAction(nameof(ObtenerCapacitacionPorId),
                new { id = capacitacion.IdCapacitacion },
                resumen);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarCapacitacion(int id, [FromBody] CapacitacionEditarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var capacitacion = await _db.Capacitaciones
                .FirstOrDefaultAsync(c => c.IdCapacitacion == id);

            if (capacitacion is null)
                return NotFound("Formación académica no encontrada");

            // (opcional) validar que lo que venga en el body coincide con la ruta
            if (dto.IdCapacitacion != 0 && dto.IdCapacitacion != id)
                return BadRequest("El Id de la capacitacion no coincide con la ruta");

            // actualiza campos (sin tocar el Id)
            _mapper.Map(dto, capacitacion);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarCapacitacion(int id)
        {
            var capacitacion = await _db.Capacitaciones
                .FirstOrDefaultAsync(c => c.IdCapacitacion == id);

            if (capacitacion is null)
                return NotFound("Capacitacion no encontrada");

            _db.Capacitaciones.Remove(capacitacion);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
