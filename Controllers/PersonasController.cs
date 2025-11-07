using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Personas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Casual")]
    [ApiController]
    public class PersonasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public PersonasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerTodasPeronas()
        {
            var personas = await _db.Personas.Include(p => p.Nacionalidad).ToListAsync();
            var listaPersonas = _mapper.Map<List<PersonasListarDTO>>(personas);
            return Ok(listaPersonas);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPersonaId(int id)
        {
            var persona = await _db.Personas.Include(p => p.Nacionalidad).FirstOrDefaultAsync(p => p.IdPersona == id);
            if (persona is null)
            {
                return NotFound("Persona no encontrada");
            }
            var personaDTO = _mapper.Map<PersonasListarDTO>(persona);
            return Ok(personaDTO);
        }
        [HttpPost]
        public async Task<IActionResult> CrearPersona(PersonaCrearDTO personaCrearDTO)
        {
            if (personaCrearDTO is null)
            {
                return BadRequest("La persona no puede ser nula");
            }
            var persona = _mapper.Map<Persona>(personaCrearDTO);
            await _db.Personas.AddAsync(persona);
            await _db.SaveChangesAsync();
            var personaCreada = _mapper.Map<PersonasListarDTO>(persona);
            return CreatedAtAction(nameof(ObtenerPersonaId), new { id = persona.IdPersona }, personaCreada);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarPersona(int id, PersonaCrearDTO personaActualizarDTO)
        {
            var persona = await _db.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);
            if (persona is null)
            {
                return NotFound("Persona no encontrada");
            }
            _mapper.Map(personaActualizarDTO, persona);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarPersona(int id)
        {
            var persona = await _db.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);
            if (persona is null)
            {
                return NotFound("Persona no encontrada");
            }
            _db.Personas.Remove(persona);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
