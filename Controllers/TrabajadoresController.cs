using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Trabajadores;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrabajadoresController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public TrabajadoresController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET: api/Trabajadores
        [HttpGet]
        public async Task<IActionResult> ObtenerTrabajadores()
        {
            var trabajadores = await _db.Trabajadores
                .Include(t => t.Persona)
                .ThenInclude(p => p.Nacionalidad)
                .Include(t => t.Cargo)
                .ThenInclude(c => c.Oficina)
                .Include(t => t.Horarios)
                .ToListAsync();

            var listTrabajadores = _mapper.Map<List<TrabajadoresListarDTO>>(trabajadores);
            return Ok(listTrabajadores);
        }

        // GET: api/Trabajadores/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerTrabajadorId(int id)
        {
            var trabajador = await _db.Trabajadores
                .Include(t => t.Persona)
                .ThenInclude(p => p.Nacionalidad)
                .Include(t => t.Cargo)
                .ThenInclude(c => c.Oficina)
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.IdTrabajador == id);

            if (trabajador is null)
                return NotFound("Trabajador no encontrado");

            var trabajadorDTO = _mapper.Map<TrabajadoresListarDTO>(trabajador);
            return Ok(trabajadorDTO);
        }

        // POST: api/Trabajadores
        [HttpPost]
        public async Task<IActionResult> CrearTrabajador(TrabajadorCrearDTO trabajadorCrearDTO)
        {
            if (trabajadorCrearDTO is null)
                return BadRequest("El trabajador no puede ser nulo");

            // Validar duplicados dentro del DTO
            var diasDuplicados = trabajadorCrearDTO.Horarios
                .GroupBy(h => h.DiaSemana)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (diasDuplicados.Any())
                return BadRequest($"No se puede tener horarios duplicados para los días: {string.Join(", ", diasDuplicados)}");

            // Mapear trabajador
            var trabajador = _mapper.Map<Trabajador>(trabajadorCrearDTO);

            // Guardar el trabajador primero para generar su Id
            await _db.Trabajadores.AddAsync(trabajador);
            await _db.SaveChangesAsync();

            // Eliminar horarios previos si existieran (para evitar duplicados)
            var horariosExistentes = _db.Horarios.Where(h => h.IdTrabajador == trabajador.IdTrabajador);
            _db.Horarios.RemoveRange(horariosExistentes);
            await _db.SaveChangesAsync();

            // Agregar los nuevos horarios
            foreach (var horarioDTO in trabajadorCrearDTO.Horarios)
            {
                var horario = _mapper.Map<Horario>(horarioDTO);
                horario.IdTrabajador = trabajador.IdTrabajador; // asignar FK correcta
                await _db.Horarios.AddAsync(horario);
            }

            await _db.SaveChangesAsync();

            var trabajadorDTO = _mapper.Map<TrabajadoresListarDTO>(trabajador);
            return Ok(trabajadorDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarTrabajador(int id)
        {
            var trabajador = await _db.Trabajadores
                .Include(t => t.Horarios) // cargar horarios para eliminarlos
                .FirstOrDefaultAsync(t => t.IdTrabajador == id);

            if (trabajador == null)
                return NotFound("Trabajador no encontrado");

            // Eliminar horarios asociados primero
            if (trabajador.Horarios.Any())
                _db.Horarios.RemoveRange(trabajador.Horarios);

            // Eliminar trabajador
            _db.Trabajadores.Remove(trabajador);

            await _db.SaveChangesAsync();

            return Ok("Trabajador eliminado correctamente");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarTrabajador(int id, TrabajadorCrearDTO trabajadorActualizarDTO)
        {
            if (trabajadorActualizarDTO is null)
                return BadRequest("El trabajador no puede ser nulo");

            // Buscar el trabajador existente
            var trabajador = await _db.Trabajadores
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.IdTrabajador == id);

            if (trabajador == null)
                return NotFound("Trabajador no encontrado");

            // Validar duplicados en los horarios del DTO
            var diasDuplicados = trabajadorActualizarDTO.Horarios
                .GroupBy(h => h.DiaSemana)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (diasDuplicados.Any())
                return BadRequest($"No se puede tener horarios duplicados para los días: {string.Join(", ", diasDuplicados)}");

            // Actualizar campos del trabajador
            trabajador.IdPersona = trabajadorActualizarDTO.IdPersona;
            trabajador.HaberBasico = trabajadorActualizarDTO.HaberBasico;
            trabajador.FechaIngreso = trabajadorActualizarDTO.FechaIngreso;
            trabajador.IdCargo = trabajadorActualizarDTO.IdCargo;

            // Eliminar horarios existentes
            if (trabajador.Horarios.Any())
                _db.Horarios.RemoveRange(trabajador.Horarios);

            // Agregar los nuevos horarios
            foreach (var horarioDTO in trabajadorActualizarDTO.Horarios)
            {
                var horario = _mapper.Map<Horario>(horarioDTO);
                horario.IdTrabajador = trabajador.IdTrabajador;
                await _db.Horarios.AddAsync(horario);
            }

            await _db.SaveChangesAsync();

            var trabajadorDTO = _mapper.Map<TrabajadoresListarDTO>(trabajador);
            return Ok(trabajadorDTO);
        }

    }
}
